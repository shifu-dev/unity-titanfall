using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameLog
{
    public class Logger : ILogger
    {
        //////////////////////////////////////////////////////////////////
        /// Constants
        //////////////////////////////////////////////////////////////////

        public const LogLevel ThrowErrorLevel = LogLevel.CRITICAL;
        public const int DefaultLogTargetsCapacity = 2;

        //////////////////////////////////////////////////////////////////
        /// Variables
        //////////////////////////////////////////////////////////////////

        public ILogger ilogger => (ILogger)this;

        public string loggerName { get; set; } = "UnknownLogger";

        public LogLevel logLevel { get; set; } = LogLevel.INFO;

        public LogLevel flushLevel { get; set; } = LogLevel.INFO;

        public List<ILogTarget> logTargets { get; set; } = new List<ILogTarget>(DefaultLogTargetsCapacity);

        //////////////////////////////////////////////////////////////////
        /// Constructors
        //////////////////////////////////////////////////////////////////

        public Logger(string name, params ILogTarget[] logTargets)
        {
            this.loggerName = name;
            this.logTargets = new List<ILogTarget>(logTargets);
        }

        public Logger(string name, LogLevel logLevel, params ILogTarget[] logTargets)
        {
            this.loggerName = name;
            this.logLevel = logLevel;
            this.logTargets = new List<ILogTarget>(logTargets);
        }

        //////////////////////////////////////////////////////////////////
        /// ILogger Interface
        //////////////////////////////////////////////////////////////////

        public void Log(LogLevel lvl, string format, params object[] msg)
        {
            if (ilogger.CanLog(lvl))
            {
                InternalLog(lvl, format, msg);
            }

            if (ilogger.CanFlush(lvl))
            {
                Flush(lvl);
            }
        }

        public void InternalLog(LogLevel lvl, string format, params object[] msg)
        {
            if (logTargets.Count == 0)
                return;

            LogMsg logMsg = new LogMsg(lvl, loggerName, format, msg);

            foreach (var logTarget in logTargets)
            {
                logTarget.Log(logMsg);
            }
        }

        public void Write(LogLevel lvl, string msg)
        {
            if (ilogger.CanLog(lvl))
            {
                foreach (var logTarget in logTargets)
                {
                    logTarget.Write(lvl, msg);
                }
            }

            if (ilogger.CanFlush(lvl))
            {
                Flush(lvl);
            }
        }

        public void Assert(bool assertion, LogLevel lvl, string format, params object[] msg)
        {
            if (assertion == false)
            {
                Log(lvl, format, msg);
                throw new UnityException(String.Format(format, msg));
            }
        }

        public void Flush(LogLevel lvl)
        {
            if (ilogger.CanFlush(lvl))
            {
                foreach (var logTarget in logTargets)
                {
                    logTarget.Flush(lvl);
                }
            }
        }

        public void ForceFlush()
        {
            foreach (var logTarget in logTargets)
            {
                logTarget.ForceFlush();
            }
        }
    }
}