using System;
using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// プレイヤの近くに飛行機を墜落させる
    /// </summary>
    [ParupunteConfigAttribute("メーデー！メーデー！メーデー！")]
    internal class Mayday : ParupunteScript
    {
        public Mayday(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            StartCoroutine(AirPlaneCoroutine());
        }

        private IEnumerable<object> AirPlaneCoroutine()
        {
            //飛行機生成
            var model = new Model(VehicleHash.Jet);
            var plane = GTA.World.CreateVehicle(model, core.PlayerPed.Position + new Vector3(0, 0, 100));
            if (!plane.IsSafeExist()) yield break;
            plane.Speed = 0;
            plane.MarkAsNoLongerNeeded();

            //ラマー生成
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            ped.MarkAsNoLongerNeeded();
            ped.Task.ClearAll();

            foreach (var s in WaitForSeconds(8))
            {
                var length = (core.PlayerPed.Position - plane.Position).Length();
                if (length < 400.0f) break;
                yield return null;
            }

            if (!plane.IsSafeExist() || !ped.IsSafeExist()) yield break;
            plane.EngineHealth = 0;
            plane.EngineRunning = false;

            //飛行機が壊れたら大爆発させる
            foreach (var s in WaitForSeconds(10))
            {
                if (!plane.IsSafeExist()) break;
                if (!plane.IsAlive)
                {
                    foreach (var i in Enumerable.Range(0, 10))
                    {
                        if (!plane.IsSafeExist()) break;
                        var point = plane.Position.Around(10.0f);
                        GTA.World.AddExplosion(point, GTA.ExplosionType.Rocket, 20.0f, 1.5f);
                        yield return WaitForSeconds(0.2f);
                    }
                    break;
                }
                yield return null;
            }

            DeletePlaneAfter(plane).Forget(); //TODO: CancellationToken
            ParupunteEnd();
        }

        private async Task DeletePlaneAfter(Vehicle plane, CancellationToken token = default(CancellationToken))
        {
            await Task.Delay(TimeSpan.FromSeconds(10), token);
            if (plane.IsSafeExist())
            {
                plane.Delete();
            }
        }
    }
}
