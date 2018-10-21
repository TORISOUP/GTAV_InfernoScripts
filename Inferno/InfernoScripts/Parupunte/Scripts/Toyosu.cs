using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("あぁ^～豊洲が漁業するんじゃぁ^～", "大漁")]
    [ParupunteIsono("とよす")]
    internal class Toyosu : ParupunteScript
    {
        private HashSet<Vehicle> vehicleList = new HashSet<Vehicle>();

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
            var playerViecle = player.CurrentVehicle;
            var targets = core.CachedVehicles
                .Where(x => x.IsSafeExist()
                            && x.IsInRangeOf(player.Position, 30.0f)
                            && x != playerViecle
                );

            foreach (var v in targets)
            {
                if (!vehicleList.Contains(v))
                {
                    vehicleList.Add(v);
                    StartCoroutine(VehiclePyonPyon(v));
                }
            }
        }

        private IEnumerable<object> VehiclePyonPyon(Vehicle v)
        {
            yield return core.CreateRadomWait();
            while (!ReduceCounter.IsCompleted && v.IsSafeExist())
            {
                if (!v.IsSafeExist() || !v.IsAlive) yield break;
                if (!v.IsInRangeOf(core.PlayerPed.Position, 30.0f))
                {
                    yield return null;
                    continue;
                }

                var toPlayer = core.PlayerPed.Position - v.Position;
                toPlayer.Normalize();
                var power = v.IsInRangeOf(core.PlayerPed.Position, 7) ? 0 : 2;
                v.ApplyForce(Vector3.WorldUp * 10 + toPlayer * power, Vector3.RandomXYZ() * 10);

                foreach (var w in WaitForSeconds(0.4f))
                {
                    v.ApplyForce(Vector3.WorldDown * 5);

                    yield return null;
                }
            }
        }
    }
}
