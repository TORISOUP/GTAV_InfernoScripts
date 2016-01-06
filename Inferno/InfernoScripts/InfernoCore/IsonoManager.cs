using Inferno.Isono;
using UniRx;

namespace Inferno
{
    //ISONO管理マネージャ
    public class IsonoManager : InfernoScript
    {
        public static IsonoManager Instance { get; private set; }

        public UniRx.IObservable<string> OnRecievedMessageAsObservable
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
