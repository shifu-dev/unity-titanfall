using System;
using System.Collections.Generic;

namespace GameLog
{
    public struct LogMsg
    {
        public LogLevel logLevel;
        public List<string> loggerNames;
        public string msg;

        public LogMsg(LogLevel lvl, string loggerName, string msg)
        {
            this.logLevel = lvl;
            this.loggerNames = new List<string>(1);
            this.loggerNames.Add(loggerName);
            this.msg = msg;
        }

        public LogMsg(LogLevel lvl, string loggerName, string format, params object[] msg)
        {
            this.logLevel = lvl;
            this.loggerNames = new List<string>(1);
            this.loggerNames.Add(loggerName);
            this.msg = String.Format(format, msg);
        }

        public LogMsg(LogLevel lvl, string[] loggerNames, string format, params object[] msg)
        {
            this.logLevel = lvl;
            this.loggerNames = new List<string>(loggerNames);
            this.msg = String.Format(format, msg);
        }
    }
}