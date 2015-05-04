using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{
    public class DebugLogger
    {
        private TcpSocketManager tcpSocketManager;

        public DebugLogger()
        {
            tcpSocketManager = new TcpSocketManager();
            tcpSocketManager.ServerStart();
        }

        /// <summary>
        /// メッセージをブロードキャストする
        /// </summary>
        /// <param name="message"></param>
        private void Write(string message)
        {
            tcpSocketManager.SendToAll(message);
        }

        public void Log(string message)
        {
            var sendMessage = String.Format("[{0}] {1}", DateTime.Now, message);
            Write(sendMessage);
        }

    }

}
