using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("あぁ^～豊洲が漁業するんじゃぁ^～", "大漁")]
    [ParupunteIsono("とよす")]
    internal class Toyosu : ParupunteScript
    {
        private readonly HashSet<Vehicle> vehicleList = new();

        public Toyosu(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }

        protected override void OnUpdate()
        {
            var player = core.PlayerPed;
            var targets = core.CachedVehicles
                .Where(x => x.IsSafeExist()
                            && x.IsInRangeOf(player.Position, 30.0f)
                );

            foreach (var v in targets)
            {
                if (vehicleList.Add(v))
                {
                    ToyosuAsync(v, ActiveCancellationToken).Forget();
                }
            }
        }

        private async ValueTask ToyosuAsync(Vehicle v, CancellationToken ct)
        {
            await DelayRandomFrameAsync(1, 10, ct);
            var count = 0;
            while (!ReduceCounter.IsCompleted && v.IsSafeExist() && !ct.IsCancellationRequested)
            {
                if (!v.IsSafeExist() || !v.IsAlive)
                {
                    return;
                }

                if (v == core.PlayerPed.CurrentVehicle)
                {
                    await Delay100MsAsync(ct);
                    continue;
                }

                if (!v.IsInRangeOf(core.PlayerPed.Position, 30.0f))
                {
                    await Delay100MsAsync(ct);
                    continue;
                }

                var toPlayer = core.PlayerPed.Position - v.Position;
                toPlayer.Normalize();
                var power = v.IsInRangeOf(core.PlayerPed.Position, 7) ? 0 : 2;

                if (count++ % 2 == 0)
                {
                    v.ApplyForce(Vector3.WorldDown * 15);
                    await DelaySecondsAsync(0.2f, ct);
                }
                else
                {
                    v.ApplyForce(Vector3.WorldUp * 10 + toPlayer * power, Vector3.RandomXYZ() * 10);
                    await DelaySecondsAsync(0.1f, ct);
                }

            }
        }
    }
}