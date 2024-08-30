using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("磯野～！緊急脱出しようぜ！")]
    [ParupunteIsono("きんきゅうだっしゅつ")]
    internal class CitizenEmergencyEscape : ParupunteScript
    {
        public CitizenEmergencyEscape(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Concat(new[] { core.PlayerPed })
                         .Where(
                             x => x.IsSafeExist()
                                  && x.IsAlive
                                  && x.IsInVehicle()
                                  && !x.IsCutsceneOnlyPed()
                                  && x.IsInRangeOf(playerPos, 100)
                         ))
            {
                var car = ped.CurrentVehicle;
                if (car.IsSafeExist() && car.PetrolTankHealth > 0 && !car.IsRequiredForMission())
                {
                    car.PetrolTankHealth = -700;
                }

                DelayParachuteAsync(ped, ActiveCancellationToken).Forget();
            }


            DelayFinishedAsync(ActiveCancellationToken).Forget();
        }


        private async ValueTask DelayParachuteAsync(Ped ped, CancellationToken ct)
        {
            var isPedNotChaos = ped.IsNotChaosPed();
            try
            {
                ped.SetNotChaosPed(true);
                ped.ClearTasksImmediately();
                ped.Position += new Vector3(0, 0, 0.5f);
                ped.SetToRagdoll();
                await Delay100MsAsync(ct);
                ped.ApplyForce(new Vector3(0, 0, 40.0f));

                await DelaySecondsAsync(1f, ct);
                if (!ped.IsSafeExist())
                {
                    return;
                }

                ped.ParachuteTo(core.PlayerPed.Position);
                ped.AlwaysKeepTask = true;

                for (int i = 0; i < 10; i++)
                {
                    if (!ped.IsSafeExist())
                    {
                        return;
                    }

                    if (ped.IsInAir)
                    {
                        await DelaySecondsAsync(1, ct);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    ped.SetNotChaosPed(isPedNotChaos);
                }
            }
        }


        private async ValueTask DelayFinishedAsync(CancellationToken ct)
        {
            await DelaySecondsAsync(7, ct);
            ParupunteEnd();
        }
    }
}