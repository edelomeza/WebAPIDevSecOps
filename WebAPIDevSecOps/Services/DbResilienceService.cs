using Microsoft.Extensions.Options;
using WebAPIDevSecOps.Context;
using WebAPIDevSecOps.Dto;
using Polly;
using Polly.CircuitBreaker;

namespace WebAPIDevSecOps.Services
{
    public class DbResilienceService
    {
        private readonly ResiliencePipeline _pipeline;
        private readonly ILogger<DbResilienceService> _logger;
        private long _failureCount;

        public DbResilienceService(IOptions<ResilienceOptions> options, ILogger<DbResilienceService> logger)
        {
            _logger = logger;

            _pipeline = new ResiliencePipelineBuilder()
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = options.Value.FailureRatio,
                    SamplingDuration = TimeSpan.FromSeconds(options.Value.SamplingDurationSeconds),
                    MinimumThroughput = options.Value.MinimumThroughput,
                    BreakDuration = TimeSpan.FromSeconds(options.Value.BreakDurationSeconds),
                    OnOpened = args =>
                    {
                        var count = Interlocked.Read(ref _failureCount);
                        _logger.LogWarning("Circuit breaker abierto por {BreakDuration}s tras {FailureCount} fallos",
                            args.BreakDuration.TotalSeconds, count);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        Interlocked.Exchange(ref _failureCount, 0);
                        _logger.LogInformation("Circuit breaker cerrado tras recuperación");
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = args =>
                    {
                        _logger.LogInformation("Circuit breaker en modo half-open — probando recuperación");
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        public async Task<int> SaveChangesAsync(AppDbContext context, CancellationToken ct = default)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                return await context.SaveChangesAsync(token);
            }, ct);
        }
    }
}
