using System.Diagnostics;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shorty.Api.HostedServices;
using Shorty.Api.Middleware;
using Shorty.Application;
using Shorty.Application.PipelineBehaviors;
using Shorty.Application.Services;
using Shorty.Db;
using Shorty.Db.Migrations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

builder.Services.AddHostedService<DbMigratorHostedService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var dbConnectionStr = builder.Configuration.GetConnectionString("ShortyDb") ?? throw new UnreachableException("ShortyDb connection string is not configured");
builder.Services.AddDbContext<AppDbContext>(optionsBuilder => { optionsBuilder.UseNpgsql(dbConnectionStr); });

builder.Services.AddSingleton<IDbMigrationEngine, DbMigrationEngine>(_ => new DbMigrationEngine(dbConnectionStr));

builder.Services.AddValidatorsFromAssemblyContaining<CreateUrlGroupCommandValidator>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
builder.Services.AddMediatR(m => { m.RegisterServicesFromAssemblyContaining<CreateUrlGroupCommandHandler>(); });

builder.Services.AddSingleton<IRandomGenerator, RandomGenerator>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(opt => { });

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
    {
        var dbMigrationEngine = app.Services.GetRequiredService<IDbMigrationEngine>();
        dbMigrationEngine.Migrate();
    });

app.Run();