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
            var loggerNames = logMsg.loggerNames;
            StringBuilder fmtLoggerName = new StringBuilder(loggerNames.Count);
            foreach (var loggerName in loggerNames)
            {
                fmtLoggerName.Append(loggerName);
                fmtLoggerName.Append(" | ");
            }

            return String.Format("{0} [{1}]: {2}", fmtLoggerName, logMsg.logLevel, logMsg.msg);
        }

        protected abstract void InternalWrite(LogLevel lvl, string msg);
        protected abstract void InternalFlush();
    }
}