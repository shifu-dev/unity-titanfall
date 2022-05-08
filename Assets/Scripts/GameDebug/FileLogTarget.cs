using System.IO;

namespace GameLog
{
    public class FileLogTarget : LogTarget
    {
        public StreamWriter fileStreamWriter { get; private set; }
        public FileStream fileStream => (FileStream)fileStreamWriter.BaseStream;

        public FileLogTarget(string filePath, FileMode fileMode = FileMode.Create)
            : base(LogLevel.INFO, LogLevel.INFO)
        {
            var fileStream = new FileStream(filePath, fileMode);
            fileStreamWriter = new StreamWriter(fileStream);
        }

        ~FileLogTarget()
        {
            fileStreamWriter.Close();
        }

        protected override void InternalWrite(LogLevel lvl, string msg)
        {
            fileStreamWriter.Write(msg);
        }

        protected override void InternalFlush()
        {
            fileStreamWriter.Flush();
        }
    }
}