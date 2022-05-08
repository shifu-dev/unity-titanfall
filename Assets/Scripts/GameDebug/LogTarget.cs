using System;
using System.Text;

namespace GameLog
{
    public interface ILogTarget
    {
        LogLevel logLevel { get; set; }
        LogLevel flushLevel { get; set; }

        bool CanLog(LogMsg logMsg) => logMsg.logLevel >= logLevel;
        bool CanLog(LogLevel lvl) => lvl >= logLevel;
        bool CanFlush(LogLevel lvl) => lvl >= flushLevel;

        void Log(LogMsg logMsg);
        void Write(LogLevel lvl, string msg);

        void Flush(LogLevel lvl);
        void ForceFlush();
    }

    public abstract class LogTarget : ILogTarget
    {
        public ILogTarget iLogTarget => (ILogTarget)this;

        public LogLevel logLevel { get; set; }
        public LogLevel flushLevel { get; set; }

        public LogTarget(LogLevel logLevel, LogLevel flushLevel)
        {
            this.logLevel = logLevel;
            this.flushLevel = flushLevel;
        }

        // Formats the LogMsg and writes
        public void Log(LogMsg logMsg)
        {
            if (iLogTarget.CanLog(logMsg) == false)
                return;

            string fmtMsg = Format(logMsg);
            InternalWrite(logMsg.logLevel, fmtMsg);
        }

        // Writes down to the target, does not format the msg
        public void Write(LogLevel lvl, string msg)
        {
            if (iLogTarget.CanLog(lvl) == false)
                return;

            InternalWrite(lvl, msg);
        }

        public void Flush(LogLevel lvl)
        {
            if (iLogTarget.CanFlush(lvl) == false)
                return;

            InternalFlush();
        }

        public void ForceFlush()
        {
            InternalFlush();
        }

        protected virtual string Format(LogMsg logMsg)
        {
            // Join All LoggerNames
            StringBuilder fmtLoggerName = new StringBuilder(String.Empty);
            var loggerNames = logMsg.loggerNames;
            if (loggerNames.Count > 0)
            {
                fmtLoggerName.Clear();

                int length = loggerNames.Count;
                fmtLoggerName.Append(loggerNames[length - 1]);
                for (int i = length - 2; i >= 0; i--)
                {
                    fmtLoggerName.Append(" | ");
                    fmtLoggerName.Append(loggerNames[i]);
                }
            }

            return String.Format($"[{logMsg.logLevel}] {fmtLoggerName}: {logMsg.msg}\n");
        }

        protected abstract void InternalWrite(LogLevel lvl, string msg);
        protected abstract void InternalFlush();
    }
}