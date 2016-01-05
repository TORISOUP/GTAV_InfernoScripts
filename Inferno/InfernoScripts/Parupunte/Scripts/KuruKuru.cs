using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using GTA; using UniRx;
using GTA.Math;
using Inferno.InfernoScripts.Parupunte;

namespace Inferno
{
    class KuruKuru : ParupunteScript
    {

        private IDisposable mainStream;
        public KuruKuru(ParupunteCore core) : base(core)
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
        }

        public override string Name { get; } = "くるくる";
        public override string EndMessage { get; } = "おわり";

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            AddProgressBar(ReduceCounter);

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                var player = core.PlayerPed;
                var targets = core.CachedVehicles
                    .Where(x => x.IsSafeExist()
                                && x.IsInRangeOf(player.Position, 80.0f)
                                && x != player.CurrentVehicle

                    );
                foreach (var veh in targets)
                {
                    veh.Speed = 200;
                }
                ParupunteEnd();
            });

            //TODO 別の場所にHookする
            mainStream = 
            DrawingCore.OnDrawingTickAsObservable
                .TakeUntil(ReduceCounter.OnFinishedAsync)
                .Subscribe(_ =>
                {

                    var player = core.PlayerPed;
                    var targets = core.CachedVehicles
                        .Where(x => x.IsSafeExist()
                                    && x.IsInRangeOf(player.Position, 80.0f)
                                    && x != player.CurrentVehicle

                        );
                    var rate = (1.0f - ReduceCounter.Rate);
                    foreach (var veh in targets)
                    {
                        if(!veh.IsSafeExist()) continue;
                        veh.Quaternion = Quaternion.RotationAxis(Vector3.WorldUp, 1.0f * rate) * veh.Quaternion;  
                        if (rate > 0.5f)
                        {
                            veh.ApplyForce(Vector3.WorldUp * 2.0f * rate);
                            veh.Speed = 40.0f*2.0f*(rate - 0.5f);
                        }
                    }
                });
        }

        protected override void OnFinished()
        {
            mainStream?.Dispose();
        }
    }
}
