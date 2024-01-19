using System.Diagnostics;
using EasyCaching.Core;
using EasyCaching.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shorty.Db;
using Shorty.Db.Migrations;
using Shorty.RedirectApi.HostedServices;
using Shorty.RedirectApi.RateLimitPolicies;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<DbMigratorHostedService>();

var shortyDbConnectionStr = builder.Configuration.GetConnectionString("ShortyDb")
                            ?? throw new UnreachableException("ShortyDb connection string is not configured");
var shortyRedisConnectionStr = builder.Configuration.GetConnectionString("ShortyRedis")
                               ?? throw new UnreachableException("ShortyRedis connection string is not configured");

builder.Services.AddDbContext<AppDbContext>(optionsBuilder => { optionsBuilder.UseNpgsql(shortyDbConnectionStr); });
builder.Services.AddSingleton<IDbMigrationEngine, DbMigrationEngine>(_ => new DbMigrationEngine(shortyDbConnectionStr));

builder.Services.AddEasyCaching(options =>
    {
        options.WithJson("json-serializer");
        options.UseInMemory(memoryOptions => { memoryOptions.EnableLogging = true; }, "m");
        options.UseRedis(redisOptions =>
            {
                redisOptions.EnableLogging = true;
                redisOptions.DBConfig = new RedisDBOptions
                {
                    Configuration = shortyRedisConnectionStr
                };
                redisOptions.SerializerName = "json-serializer";
            }, "r");

        options.UseHybrid(hybridCachingOptions =>
                {
                    hybridCachingOptions.EnableLogging = true;
                    hybridCachingOptions.TopicName = "hybrid-caching-api--topic";
                    hybridCachingOptions.LocalCacheProviderName = "m";
                    hybridCachingOptions.DistributedCacheProviderName = "r";
                })
            .WithRedisBus(busOptions =>
                {
                    busOptions.Configuration = shortyRedisConnectionStr;
                    busOptions.SerializerName = "json-serializer";
                });
    });

var connectionMultiplexer = ConnectionMultiplexer.Connect(shortyRedisConnectionStr);
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => connectionMultiplexer);
builder.Services.AddRateLimiter(options => { options.AddPolicy<string, IpRateLimitPolicy>("ip-rate-limit-policy"); });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Append("X-Machine-Name", Environment.MachineName);
                    return Task.CompletedTask;
                });

            await next();
        }
);

app.UseSwagger();
app.UseSwaggerUI();

app.UseRateLimiter();

app.MapGet("/{groupName}/{shortUrl}", Handler)
    .WithName("RedirectToUrl")
    .WithOpenApi()
    .RequireRateLimiting("ip-rate-limit-policy")
    ;

app.Run();
return;

static async Task<IResult> Handler([FromRoute] string groupName, [FromRoute] string shortUrl, [FromServices] AppDbContext dbContext, [FromServices] IHybridCachingProvider cachingProvider, CancellationToken cancellationToken)
{
    string key = $"{groupName}:{shortUrl}";
    var cacheValue = cachingProvider.Get<string>(key);
    if (cacheValue.HasValue)
    {
        return Results.Redirect(cacheValue.Value);
    }

    var urlGroup = await dbContext.UrlGroups.FirstOrDefaultAsync(group => group.Name == groupName);
    if (urlGroup == null)
    {
        return Results.NotFound("Url group not found");
    }

    var url = await dbContext.Urls.FirstOrDefaultAsync(u => u.UrlGroupId == urlGroup.Id && u.ShortUrl == shortUrl);
    if (url == null)
    {
        return Results.NotFound("Url not found");
    }

    await cachingProvider.SetAsync(key, url.OriginalUrl, TimeSpan.FromMinutes(5));
    return Results.Redirect(url.OriginalUrl);
}