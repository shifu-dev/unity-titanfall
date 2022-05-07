using UnityEngine;

namespace GameLog
{
    public class ConsoleLogTarget : LogTarget
    {
        public ConsoleLogTarget()
            : base(LogLevel.INFO, LogLevel.WARN) { }

        protected override void InternalWrite(LogLevel lvl, string msg)
        {
            if (lvl == LogLevel.WARN)
            {
                Debug.LogWarning(msg);
                return;
            }

            if (lvl == LogLevel.ERR || lvl == LogLevel.CRITICAL)
            {
                Debug.LogError(msg);
                return;
            }

            Debug.Log(msg);
        }

        protected override void InternalFlush()
        {
        }
    }
}