using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Inferno;
using Inferno.Isono;
using Inferno.Utilities;

namespace Inferno
{

    //ISONO管理マネージャ
    public class IsonoManager : InfernoScript
    {
        public static IsonoManager Instance { get; private set; }

        public IObservable<string> OnRecievedMessageAsObservable
        {
            get
            {
                return IsonoTcpClient.OnRecievedMessageAsObservable;
            }
        }

        private IsonoTcpClient isonoTcpClient;

        private IsonoTcpClient IsonoTcpClient => isonoTcpClient ?? (isonoTcpClient = new IsonoTcpClient("127.0.0.1", 50082));

        protected override void Setup()
        {
            Instance = this;
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
            
        }
    }
}
