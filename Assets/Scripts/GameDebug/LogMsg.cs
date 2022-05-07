using System;

namespace GameLog
{
    public struct LogMsg
    {
        public LogLevel logLevel;
        public string loggerName;
        public string msg;

        public LogMsg(LogLevel lvl, string loggerName, string format, params object[] msg)
        {
            this.logLevel = lvl;
            this.loggerName = loggerName;
            this.msg = String.Format(format, msg);
        }
    }
}