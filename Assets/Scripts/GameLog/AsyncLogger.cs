#nullable enable

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Events;

namespace Serilog
{
    public class AsyncLogger : ILogger
    {
        ~AsyncLogger()
        {
            _dispose = true;
            _worker.Join();

            _mSyncEvent.Close();
            _aSyncEvent.Close();
        }

        public AsyncLogger(ILogger logger, bool completeTasksBeforeDisposing = true)
        {
            _completeTasksBeforeDisposing = completeTasksBeforeDisposing;

            _logger = logger;
            _dispose = false;
            _logEvents = new ConcurrentQueue<LogEvent>();
            _aSyncEvent = new AutoResetEvent(false);
            _mSyncEvent = new ManualResetEvent(false);
            _worker = new Thread(WorkerProc);

            _worker.Start();
        }

        protected void WorkerProc()
        {
            while (true)
            {
                if (_dispose)
                {
                    if (_logEvents.Count == 0 && _completeTasksBeforeDisposing == false)
                    {
                        _mSyncEvent.Set();
                        break;
                    }
                }

                if (_logEvents.TryDequeue(out LogEvent logEvent))
                {
                    try
                    {
                        _logger.Write(logEvent);
                    }
                    catch (Exception) { }
                }
                else
                {
                    _mSyncEvent.Set();
                    _aSyncEvent.WaitOne();
                    _mSyncEvent.Reset();
                }
            }
        }

        public void Wait()
        {
            _mSyncEvent.WaitOne();
        }

        [MessageTemplateFormatMethod("messageTemplate")]
        public bool BindMessageTemplate(string messageTemplate, object?[]? propertyValues, [NotNullWhen(true)] out MessageTemplate? parsedTemplate, [NotNullWhen(true)] out IEnumerable<LogEventProperty>? boundProperties)
        {
            return _logger.BindMessageTemplate(messageTemplate, propertyValues, out parsedTemplate, out boundProperties);
        }

        public bool BindProperty(string? propertyName, object? value, bool destructureObjects, [NotNullWhen(true)] out LogEventProperty? property)
        {
            return _logger.BindProperty(propertyName, value, destructureObjects, out property);
        }

        public void Write(LogEvent logEvent)
        {
            _logEvents.Enqueue(logEvent);
            _aSyncEvent.Set();
        }

        protected readonly ConcurrentQueue<LogEvent> _logEvents;
        protected readonly AutoResetEvent _aSyncEvent;
        protected readonly ManualResetEvent _mSyncEvent;
        protected readonly Thread _worker;
        protected readonly ILogger _logger;
        protected readonly bool _completeTasksBeforeDisposing;
        protected bool _dispose;
    }
}

namespace Serilog
{
    public static class AsyncLoggerExtensions
    {
        public static ILogger CreateAsyncLogger(this LoggerConfiguration configuration, bool completeTasksBeforeDisposing = true)
        {
            return new AsyncLogger(configuration.CreateLogger(), completeTasksBeforeDisposing);
        }
    }
}