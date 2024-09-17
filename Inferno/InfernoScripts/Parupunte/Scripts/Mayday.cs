using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
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
            AirPlaneAync(ActiveCancellationToken).Forget();
        }

        private async ValueTask AirPlaneAync(CancellationToken ct)
        {
            try
            {
                //飛行機生成
                var model = new Model(VehicleHash.Jet);
                var plane = GTA.World.CreateVehicle(model, core.PlayerPed.Position + new Vector3(0, 0, 100));
                if (!plane.IsSafeExist())
                {
                    return;
                }

                plane.SetForwardSpeed(0);
                plane.IsRequiredForMission();

                //ラマー生成
                var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
                ped.IsRequiredForMission();
                ped.Task.ClearAll();
                ped.Task.DriveTo(plane, core.PlayerPed.Position, 10.0f, 60.0f, DrivingStyle.Rushed);

                AutoReleaseOnParupunteEnd(ped);
                AutoReleaseOnParupunteEnd(plane);

                var targetTime = core.ElapsedTime + 10f;

                while (core.ElapsedTime < targetTime)
                {
                    var length = (core.PlayerPed.Position - plane.Position).Length();
                    if (length < 400.0f)
                    {
                        break;
                    }

                    await Delay100MsAsync(ct);
                }

                if (!plane.IsSafeExist() || !ped.IsSafeExist())
                {
                    return;
                }

                plane.EngineHealth = 0;
                plane.IsEngineRunning = false;

                //飛行機が壊れたら大爆発させる
                targetTime = core.ElapsedTime + 10f;
                while (core.ElapsedTime < targetTime)
                {
                    if (!plane.IsSafeExist())
                    {
                        break;
                    }

                    if (!plane.IsAlive)
                    {
                        foreach (var i in Enumerable.Range(0, 10))
                        {
                            if (!plane.IsSafeExist())
                            {
                                break;
                            }

                            var point = plane.Position.Around(10.0f);
                            GTA.World.AddExplosion(point, GTA.ExplosionType.AirDefense, 20.0f, 1.5f);
                            await DelaySecondsAsync(0.2f, ct);
                        }

                        break;
                    }

                    await Delay100MsAsync(ct);
                }

                if (plane.IsSafeExist())
                {
                    plane.MarkAsNoLongerNeeded();
                }

                if (ped.IsSafeExist())
                {
                    ped.MarkAsNoLongerNeeded();
                }
            }
            finally
            {
                ParupunteEnd();
            }
        }
    }
}