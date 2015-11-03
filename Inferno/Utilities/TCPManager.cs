using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{
    class TCPManager
    {
        int port = 50085;
        TcpListener listener;
        List<TcpClient> tcpClients;
        System.Text.Encoding encoding;

        public TCPManager()
        {
            tcpClients = new List<TcpClient>();
            encoding = System.Text.Encoding.UTF8;
        }

        public void ServerStart()
        {

            this.listener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), port);
            this.listener.Start();
            Debug.Print("待受を開始しました");
            Accept();

        }

        /// <summary>
        /// 外部からの接続を待機する
        /// </summary>
        /// <returns></returns>
        async Task Accept()
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                client.NoDelay = true;
                tcpClients.Add(client);
                Debug.Print("接続{0}", ((IPEndPoint)(client.Client.RemoteEndPoint)).Address);
            }
        }

        public void Disconnect()
        {
            listener.Stop();
        }

        /// <summary>
        /// 全てのクライアントにMessageをブロードキャストする
        /// </summary>
        /// <param name="client_data"></param>
        /// <param name="message"></param>
        public void SendToAll(string message)
        {
            //接続が切れているクライアントは除去する
            var closedClients = tcpClients.Where(x => !x.Connected).ToList();
            closedClients.ForEach(x => tcpClients.Remove(x));

            foreach (var client in tcpClients)
            {
                //接続が切れていないか再確認
                if (!client.Connected) { continue; }
                var ns = client.GetStream();
                byte[] message_byte = encoding.GetBytes(message);
                try
                {
                    do
                    {
                        ns.WriteAsync(message_byte, 0, message_byte.Length);

                    } while (ns.DataAvailable);
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                    if (!client.Connected)
                    {
                        client.Close();
                    }
                }
            }
        }
    }
}
