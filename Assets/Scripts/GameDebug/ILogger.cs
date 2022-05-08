using System.Collections.Generic;

namespace GameLog
{
    public enum LogLevel
    {
        TRACE,
        INFO,
        DEBUG,
        WARN,
        ERR,
        CRITICAL,
        OFF
    }

    public interface ILogger
    {
        string loggerName { get; }
        LogLevel logLevel { get; set; }
        LogLevel flushLevel { get; set; }
        List<ILogTarget> logTargets { get; set; }

        void Trace(string format, params object[] msg) => Log(LogLevel.TRACE, format, msg);
        void Info(string format, params object[] msg) => Log(LogLevel.INFO, format, msg);
        void Debug(string format, params object[] msg) => Log(LogLevel.DEBUG, format, msg);
        void Warn(string format, params object[] msg) => Log(LogLevel.WARN, format, msg);
        void Error(string format, params object[] msg) => Log(LogLevel.ERR, format, msg);
        void Critical(string format, params object[] msg) => Log(LogLevel.CRITICAL, format, msg);

        bool CanLog(LogMsg logMsg) => logMsg.logLevel >= logLevel;
        bool CanLog(LogLevel lvl) => lvl >= logLevel;
        bool CanFlush(LogLevel lvl) => lvl >= flushLevel;

        void Log(LogLevel lvl, string format, params object[] msg);
        void Log(LogMsg logMsg);
        void Write(LogLevel lvl, string msg);
        void Throw(string format, params object[] msg);
        void Assert(bool assertion, string format, params object[] msg);
        void Assert(bool assertion, LogLevel lvl, string format, params object[] msg);

        void Flush(LogLevel lvl);
        void ForceFlush();

        ILogger CreateSubLogger(string name);
    }
}