using System;
using System.Reactive.Linq;
using Inferno.InfernoScripts.Event.Isono;

namespace Inferno
{
    //ISONO管理マネージャ
    public sealed class IsonoManager : InfernoScript
    {
        private IsonoHttpServer _httpServer;

        protected override void Setup()
        {
            _httpServer = new IsonoHttpServer(11211);

            CreateInputKeywordAsObservable("isono")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Isono:" + IsActive);
                    if (IsActive)
                    {
                        _httpServer.Start();
                    }
                    else
                    {
                        _httpServer.Stop();
                    }
                });

            _httpServer.OnReceivedMessageAsObservable
                .ObserveOn(InfernoScheduler)
                .Subscribe(x => InfernoCore.Publish(x));
        }
    }
}