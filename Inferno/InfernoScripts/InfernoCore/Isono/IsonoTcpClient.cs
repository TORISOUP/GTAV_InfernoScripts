using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;

namespace Inferno.Isono
{
    //ISONO用TCPクライアント
    public class IsonoTcpClient
    {
        private string hostIp;
        private int hostPort;
        TcpClient tcpClient;
        byte[] buffer;
        private DebugLogger logger;

        public bool IsConnected
        {
            get { return this.tcpClient != null && this.tcpClient.Connected; }
        }

        private Subject<string> _onRecievedMessageSubject = new Subject<string>();

        public IObservable<string> OnRecievedMessageAsObservable => _onRecievedMessageSubject.AsObservable();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="hostIp">ホストIP</param>
        /// <param name="hostPort">ホストポート</param>
        public IsonoTcpClient(string hostIp, int hostPort)
        {
            logger = new DebugLogger("InfenrnoIsono.log");

            this.hostIp = hostIp;
            this.hostPort = hostPort;
            buffer = new byte[2048];
        }

        /// <summary>
        /// あんこちゃんに接続を試みる
        /// </summary>
        public void Connect()
        {

            var thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    tcpClient = new TcpClient(hostIp, hostPort);
                    tcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, CallBackBeginReceive, null);
                }
                catch (Exception e)
                {
                    logger.Log(e.ToString());

                }
            }));
            thread.Start();
        }

        private void CallBackBeginReceive(IAsyncResult ar)
        {
            try
            {
                if(!IsConnected) return;
                var bytes = this.tcpClient.GetStream().EndRead(ar);

                if (bytes == 0)
                {
                    //接続断
                    Disconnect();
                    return;
                }
                var request = default(RequestDataPackage);
                try
                {
                    var recievedMessage = Encoding.UTF8.GetString(buffer, 0, bytes);
                    var recievedObject = RequestDataPackage.FromJson(recievedMessage);
                    //イベント通知
                    if (recievedObject != null)
                    {
                        _onRecievedMessageSubject.OnNext(recievedObject.text);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(e.ToString());
                }
                finally
                {
                    if (IsConnected)
                    {
                        tcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, CallBackBeginReceive, null);
                    }
                }

            }
            catch (Exception e)
            {
                Disconnect();
                logger.Log(e.ToString());
            }
        }

        public void Disconnect()
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
        }
    }
}
