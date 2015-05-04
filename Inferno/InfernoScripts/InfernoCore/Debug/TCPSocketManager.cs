using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Inferno
{
    class TcpSocketManager
    {
        int port = 50083;
        TcpListener _listener;
        List<TcpClient> _tcpClients;
        Encoding _encoding;

        public TcpSocketManager()
        {
            _tcpClients = new List<TcpClient>();
            _encoding = Encoding.UTF8;
        }

        public void ServerStart()
        {

            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            _listener.Start();
            Accept();

        }

        /// <summary>
        /// 外部からの接続を待機する
        /// </summary>
        /// <returns></returns>
        async void Accept()
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                client.NoDelay = true;
                _tcpClients.Add(client);
            }
        }

        public void Disconnect()
        {
            _listener.Stop();
        }

        /// <summary>
        /// 全てのクライアントにMessageをブロードキャストする
        /// </summary>
        public void SendToAll(string message)
        {
            //接続が切れているクライアントは除去する
            var closedClients = _tcpClients.Where(x => !x.Connected).ToList();
            closedClients.ForEach(x => _tcpClients.Remove(x));

            foreach (var client in _tcpClients)
            {
                //接続が切れていないか再確認
                if (!client.Connected) { continue; }
                var ns = client.GetStream();
                var messageByte = _encoding.GetBytes(message);
                try
                {
                    do
                    {
                        ns.WriteAsync(messageByte, 0, messageByte.Length);

                    } while (ns.DataAvailable);
                }
                catch (Exception e)
                {
                    if (!client.Connected)
                    {
                        client.Close();
                    }
                }
            }
        }
    }
}
