using Serilog.Events;
using Serilog.Core.Enrichers;

namespace Serilog.Core.Enrichers
{
    public class FrameCountEnricher : ILogEventEnricher
    {
        public const string KEY = "Frame";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory
                .CreateProperty(KEY, UnityEngine.Time.frameCount.ToString(), false));
        }
    }
}

namespace Serilog
{
    public static class FrameCountEnricherExtensions
    {
        public static LoggerConfiguration WithFrameCount(this Configuration.LoggerEnrichmentConfiguration configuration)
        {
            return configuration.With(new FrameCountEnricher());
        }
    }
}