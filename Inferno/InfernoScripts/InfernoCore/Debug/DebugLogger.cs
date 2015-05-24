using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{
    public class DebugLogger
    {
        private readonly string _logPath;
        private readonly Encoding _encoding;
        public DebugLogger(string logPath)
        {
            this._logPath = logPath;
            _encoding = Encoding.GetEncoding("Shift_JIS");
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
            var sendMessage = String.Format("[{0}] {1}", DateTime.Now, message);
            WriteToText(sendMessage);
        }

    }

}
