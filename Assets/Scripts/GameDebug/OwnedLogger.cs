using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameLog
{
    public class OwnedLogger : ILogger
    {
        //////////////////////////////////////////////////////////////////
        /// Variables
        //////////////////////////////////////////////////////////////////

        public ILogger superLogger;

        public string loggerName
        {
            get;
            set;
        }

        public string superLoggerName
        {
            get => superLogger.loggerName;
        }

        public LogLevel logLevel
        {
            get => superLogger.logLevel;
            set => superLogger.logLevel = value;
        }

        public LogLevel flushLevel
        {
            get => superLogger.flushLevel;
            set => superLogger.flushLevel = value;
        }

        public List<ILogTarget> logTargets
        {
            get => superLogger.logTargets;
            set => superLogger.logTargets = value;
        }

        //////////////////////////////////////////////////////////////////
        /// Constructors
        //////////////////////////////////////////////////////////////////

        public OwnedLogger(ILogger logger, string name)
        {
            this.superLogger = logger;
            this.loggerName = name;
        }

        //////////////////////////////////////////////////////////////////
        /// ILogger Interface
        //////////////////////////////////////////////////////////////////

        public void Log(LogLevel lvl, string format, params object[] msg)
        {
            if (superLogger.CanLog(lvl))
            {
                if (superLogger.logTargets.Count == 0)
                    return;

                LogMsg logMsg = new LogMsg(lvl, loggerName, format, msg);

                foreach (var logTarget in superLogger.logTargets)
                {
                    logTarget.Log(logMsg);
                }
            }
        }

        public void Log(LogMsg logMsg)
        {
            logMsg.loggerNames.Add(loggerName);
            superLogger.Log(logMsg);
        }

        public void Write(LogLevel lvl, string msg)
        {
            if (superLogger.CanLog(lvl))
            {
                foreach (var logTarget in superLogger.logTargets)
                {
                    logTarget.Write(lvl, msg);
                }
            }

            if (superLogger.CanFlush(lvl))
            {
                Flush(lvl);
            }
        }

        public void Assert(bool assertion, string format, params object[] msg)
        {
            Assert(assertion, Logger.DefaultAssertLevel, format, msg);
        }

        public void Assert(bool assertion, LogLevel lvl, string format, params object[] msg)
        {
            if (assertion == false)
            {
                Log(lvl, format, msg);
                throw new UnityException(String.Format(format, msg));
            }
        }

        public void Throw(string format, params object[] msg)
        {
            Log(Logger.DefaultThrowLevel, format, msg);
            throw new UnityException(String.Format(format, msg));
        }

        public void Flush(LogLevel lvl)
            => superLogger.Flush(lvl);

        public void ForceFlush()
            => superLogger.ForceFlush();

        public ILogger CreateSubLogger(string name)
        {
            return new OwnedLogger(this, name);
        }
    }
}