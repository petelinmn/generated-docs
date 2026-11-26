// Example: sampling of Information logs with Microsoft.Extensions.Telemetry.
// Module: system-design/14-observability-and-slo (expert)
// Tests: dotnet test --filter "FullyQualifiedName~LogSampling"
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Observability.Examples;

public static class LogSampling
{
    private sealed class Collector(List<string> sink) : ILoggerProvider, ILogger
    {
        public ILogger CreateLogger(string categoryName) => this;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => sink.Add(formatter(state, exception));
        public void Dispose() { }
    }

    public static IReadOnlyList<string> Run(double informationProbability)
    {
        var written = new List<string>();
        using ServiceProvider provider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddProvider(new Collector(written));
                // Only this level is sampled: warnings and errors are always kept.
                builder.AddRandomProbabilisticSampler(informationProbability, LogLevel.Information);
            })
            .BuildServiceProvider();

        ILogger logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Shop");
        logger.LogInformation("order accepted");
        logger.LogWarning("payment is slow");
        return written;
    }
}
