using System;
using UnityEngine;

namespace GameLog
{
    public class FileLogTarget : ILogTarget
    {
        public ILogTarget iLogTarget => (ILogTarget)this;

        public LogLevel logLevel { get; set; }
        public LogLevel flushLevel { get; set; }

        public FileLogTarget(string filePath)
        {
        }

        // Formats the LogMsg and writes
        public void Log(LogMsg logMsg)
        {
            if (iLogTarget.CanLog(logMsg) == false)
                return;

            string fmtMsg = Format(logMsg);
            Write(logMsg.logLevel, fmtMsg);
        }

        // Writes down to the target, does not format the msg
        public void Write(LogLevel lvl, string msg)
        {
            // if (lvl == LogLevel.WARN)
            // {
            //     Debug.LogWarning(msg);
            // }
            // else if (lvl == LogLevel.ERR || lvl == LogLevel.CRITICAL)
            // {
            //     Debug.LogError(msg);
            // }
            // else
            // {
            //     Debug.Log(msg);
            // }
        }

        public void Flush(LogLevel lvl)
        {
        }

        public void ForceFlush()
        {
        }

        protected string Format(LogMsg logMsg)
        {
            return String.Format("{0} [{1}]: {2}", logMsg.loggerName, logMsg.logLevel, logMsg.msg);
        }
    }
}