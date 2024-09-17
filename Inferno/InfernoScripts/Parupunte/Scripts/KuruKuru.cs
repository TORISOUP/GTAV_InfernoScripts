using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("くるくる", "おわり")]
    [ParupunteIsono("くるくる")]
    internal class KuruKuru : ParupunteScript
    {
        public KuruKuru(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
        }

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
                    veh.SetForwardSpeed(500);
                }

                ParupunteEnd();
            });

            MainLoopAsync(ActiveCancellationToken).Forget();
        }

        async ValueTask MainLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var player = core.PlayerPed;
                var targets = core.CachedVehicles
                    .Where(x => x.IsSafeExist()
                                && x.IsInRangeOf(player.Position, 80.0f)
                    );
                var rate = 1.0f - ReduceCounter.Rate;
                foreach (var veh in targets)
                {
                    if (!veh.IsSafeExist())
                    {
                        continue;
                    }

                    if (player.CurrentVehicle == veh)
                    {
                        continue;
                    }


                    var angle = (veh.Handle % 3) switch
                    {
                        0 => Vector3.WorldUp,
                        1 => Vector3.RelativeRight,
                        2 => Vector3.RelativeFront,
                        _ => Vector3.WorldUp
                    };

                    veh.Quaternion = Quaternion.RotationAxis(angle, 1.0f * rate) * veh.Quaternion;
                    if (rate > 0.5f)
                    {
                        veh.ApplyForce(Vector3.WorldUp * 2.0f * rate);
                        veh.SetForwardSpeed(40.0f * 2.0f * (rate - 0.5f));
                    }
                }

                await YieldAsync(ct);
            }
        }
    }
}