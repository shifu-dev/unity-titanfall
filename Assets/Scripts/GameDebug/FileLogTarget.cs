using System.IO;

namespace GameLog
{
    public class FileLogTarget : LogTarget
    {
        public StreamWriter fileSteamWriter { get; private set; }
        public FileStream fileStream => (FileStream)fileSteamWriter.BaseStream;

        public FileLogTarget(string filePath, FileMode fileMode = FileMode.CreateNew)
            : base(LogLevel.INFO, LogLevel.WARN)
        {
            var fileStream = new FileStream(filePath, fileMode);
            fileSteamWriter = new StreamWriter(fileStream);
        }

        protected override void InternalWrite(LogLevel lvl, string msg)
        {
            fileSteamWriter.Write(msg);
        }

        protected override void InternalFlush()
        {
            fileSteamWriter.Flush();
        }
    }
}