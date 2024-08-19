using System;
using System.IO;
using System.Text;

namespace Inferno
{
    public class DebugLogger
    {
        private readonly Encoding _encoding;
        private readonly string _logPath;

        public DebugLogger(string logPath)
        {
            _logPath = logPath;
            _encoding = Encoding.GetEncoding("UTF-8");
        }

        /// <summary>
        /// テキストに書き出す
        /// </summary>
        /// <param name="message"></param>
        private void WriteToText(string message)
        {
            try
            {
                using (var w = new StreamWriter(_logPath, true, _encoding))
                {
                    w.WriteLineAsync(message);
                }
            }
            catch (Exception)
            {
            }
        }

        public void Log(string message)
        {
            var sendMessage = $"[{DateTime.Now}] {message}";
            WriteToText(sendMessage);
        }
    }
}