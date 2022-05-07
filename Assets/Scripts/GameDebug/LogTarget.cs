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
}