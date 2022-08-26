// copied from https://github.com/KuraiAndras/Serilog.Sinks.Unity3D.git

using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using UnityEngine;

namespace Serilog
{
    public sealed class UnityConsoleSink : ILogEventSink
    {
        private readonly ITextFormatter _formatter;

        public UnityConsoleSink(ITextFormatter formatter)
        {
            _formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            using (var buffer = new StringWriter())
            {
                _formatter.Format(logEvent, buffer);

                switch (logEvent.Level)
                {
                    case LogEventLevel.Verbose:
                    case LogEventLevel.Debug:
                    case LogEventLevel.Information:
                        Debug.Log(buffer.ToString().Trim());
                        break;

                    case LogEventLevel.Warning:
                        Debug.LogWarning(buffer.ToString().Trim());
                        break;

                    case LogEventLevel.Error:
                    case LogEventLevel.Fatal:
                        Debug.LogError(buffer.ToString().Trim());
                        break;

                    default: throw new ArgumentOutOfRangeException(nameof(logEvent.Level), "Unknown log level");
                }
            }
        }
    }
}