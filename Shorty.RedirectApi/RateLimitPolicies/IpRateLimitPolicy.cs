using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using RedisRateLimiting;
using StackExchange.Redis;

namespace Shorty.RedirectApi.RateLimitPolicies;

public class IpRateLimitPolicy : IRateLimiterPolicy<string>
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public IpRateLimitPolicy(
        IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? Guid.NewGuid().ToString();

        return RedisRateLimitPartition.GetTokenBucketRateLimiter(ipAddress,
            s => new RedisTokenBucketRateLimiterOptions()
            {
                ConnectionMultiplexerFactory = () => _connectionMultiplexer,
                TokenLimit = int.MaxValue,
                TokensPerPeriod = 5,
                ReplenishmentPeriod = TimeSpan.FromSeconds(30)
            }
        );
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }
        = (context, _) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                return ValueTask.CompletedTask;
            };
}