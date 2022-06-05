using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace GameLog
{
    public static class GameDebug
    {
        //////////////////////////////////////////////////////////////////
        /// Constants
        //////////////////////////////////////////////////////////////////

        public const string GlobalLoggerName = "GLOBAL LOGGER";

        //////////////////////////////////////////////////////////////////
        /// Variables
        //////////////////////////////////////////////////////////////////

        public static List<ILogger> loggers { get; private set; } = new List<ILogger>(5);

        public static ILogger globalLogger { get; set; } = null;
        public static ILogTarget globalLogFile { get; private set; } = null;
        public static ILogTarget consoleLog { get; private set; } = null;

        public static LogLevel logLevel { get; set; } = LogLevel.INFO;
        public static LogLevel flushLevel { get; set; } = LogLevel.WARN;

        public static string GetLogPath => Application.temporaryCachePath;

        //////////////////////////////////////////////////////////////////
        /// Initialization and Shutdown
        //////////////////////////////////////////////////////////////////

        public static void Init()
        {
            UnityEngine.Debug.Log("[INFO] GAMEDEBUG LogPath: " + GetLogPath);

            consoleLog = new ConsoleLogTarget();
            globalLogFile = new FileLogTarget(GetLogFileNameFor(GlobalLoggerName));

            globalLogger = new Logger(GlobalLoggerName, globalLogFile, consoleLog);
        }

        public static void Shutdown()
        {
        }

        //////////////////////////////////////////////////////////////////
        /// Static API
        //////////////////////////////////////////////////////////////////

        public static bool HasLogger(string name)
        {
            return GetLogger(name) != null;
        }

        public static ILogger GetLogger(string name)
        {
            foreach (var logger in loggers)
            {
                if (logger.loggerName == name)
                    return logger;
            }

            return null;
        }

        public static ILogger CreateLogger(string name, params ILogTarget[] logTargets)
        {
            ILogger logger = InternalCreateLogger(name, logTargets);
            RegisterLogger(logger);

            return logger;
        }

        public static ILogger CreateLoggerNoRegister(string name, params ILogTarget[] logTargets)
        {
            ILogger logger = InternalCreateLogger(name, logTargets);
            return logger;
        }

        private static ILogger InternalCreateLogger(string name, params ILogTarget[] logTargets)
        {
            ILogger logger = new Logger(name);
            FileLogTarget fileTarget = new FileLogTarget(GetLogFileNameFor(name), FileMode.Create);

            // Add LogTargets to Logger
            logger.logTargets.Capacity = logTargets.Length + 3;
            logger.logTargets.Add(globalLogFile);
            logger.logTargets.Add(consoleLog);
            logger.logTargets.Add(fileTarget);
            logger.logTargets.AddRange(logTargets);

            return logger;
        }

        public static ILogger GetOrCreateLogger(string name)
        {
            ILogger logger = GetLogger(name);
            if (logger == null)
            {
                logger = CreateLogger(name);
            }

            return logger;
        }

        public static bool RegisterLogger(ILogger logger)
        {
            if (HasLogger(logger.loggerName))
            {
                return false;
            }

            loggers.Add(logger);
            return true;
        }

        public static void LogAll(LogLevel lvl, string format, params object[] msg)
        {
            foreach (var logger in loggers)
            {
                logger.Log(lvl, format, msg);
            }
        }

        public static void WriteAll(LogLevel lvl, string msg)
        {
            foreach (var logger in loggers)
            {
                logger.Write(lvl, msg);
            }
        }

        public static void FlushAll(LogLevel lvl)
        {
            foreach (var logger in loggers)
            {
                logger.Flush(lvl);
            }
        }

        public static void ForceFlushAll()
        {
            foreach (var logger in loggers)
            {
                logger.ForceFlush();
            }
        }

        private static string GetLogFileNameFor(string loggerName)
        {
            string name = Path.ChangeExtension(loggerName, ".log");
            return Path.Combine(GetLogPath, name);
        }

        //////////////////////////////////////////////////////////////////
        /// GlobalLogger API
        //////////////////////////////////////////////////////////////////

        public static void Trace(string format, params object[] msg)
            => globalLogger.Trace(format, msg);

        public static void Info(string format, params object[] msg)
            => globalLogger.Info(format, msg);

        public static void Debug(string format, params object[] msg)
            => globalLogger.Debug(format, msg);

        public static void Warn(string format, params object[] msg)
            => globalLogger.Warn(format, msg);

        public static void Error(string format, params object[] msg)
            => globalLogger.Error(format, msg);

        public static void Critical(string format, params object[] msg)
            => globalLogger.Critical(format, msg);


        public static bool CanLog(LogLevel lvl)
            => globalLogger.CanLog(lvl);

        public static bool CanFlush(LogLevel lvl)
            => globalLogger.CanFlush(lvl);


        public static void Log(LogLevel lvl, string format, params object[] msg)
            => globalLogger.Log(lvl, format, msg);

        public static void Write(LogLevel lvl, string msg)
            => globalLogger.Write(lvl, msg);

        public static void Assert(bool assertion, LogLevel lvl, string format, params object[] msg)
            => globalLogger.Assert(assertion, lvl, format, msg);


        public static void Flush(LogLevel lvl)
            => globalLogger.Flush(lvl);

        public static void ForceFlush()
            => globalLogger.ForceFlush();

#if UNITY_EDITOR

        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

#endif
    }
}