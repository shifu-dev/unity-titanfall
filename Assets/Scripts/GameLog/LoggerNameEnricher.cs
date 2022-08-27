using Serilog.Events;
using Serilog.Core.Enrichers;

namespace Serilog.Core.Enrichers
{
    public class LoggerNameEnricher : ILogEventEnricher
    {
        public const string KEY = "Logger";

        public LoggerNameEnricher(string name)
        {
            _name = name;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string loggerName = _name;

            if (logEvent.Properties.TryGetValue(KEY, out LogEventPropertyValue value))
            {
                string subLoggerName = value.ToString();
                subLoggerName = subLoggerName.Substring(1, subLoggerName.Length - 2);

                loggerName = $"{loggerName}-> {subLoggerName}";
            }

            logEvent.AddOrUpdateProperty(propertyFactory
                .CreateProperty(KEY, loggerName, false));
        }

        protected readonly string _name;
    }
}

namespace Serilog
{
    public static class LoggerNameEnricherExtensions
    {
        public static LoggerConfiguration WithLoggerName(this Configuration.LoggerEnrichmentConfiguration configuration, string name)
        {
            return configuration.With(new LoggerNameEnricher(name));
        }
    }
}