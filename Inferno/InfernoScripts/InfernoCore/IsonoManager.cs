using System;
using Inferno.InfernoScripts.Event.Isono;
using Inferno.Isono;

namespace Inferno
{
    //ISONO管理マネージャ
    public class IsonoManager : InfernoScript
    {
        private IsonoTcpClient isonoTcpClient;

        private IsonoTcpClient IsonoTcpClient =>
            isonoTcpClient ?? (isonoTcpClient = new IsonoTcpClient("127.0.0.1", 50082));

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("isono")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Isono:" + IsActive);
                    if (IsActive)
                    {
                        IsonoTcpClient.Connect();
                    }
                    else
                    {
                        IsonoTcpClient.Disconnect();
                    }
                });

            IsonoTcpClient.OnRecievedMessageAsObservable
                .Subscribe(x => { InfernoCore.Publish(new IsonoMessage(x)); });
        }
    }
}