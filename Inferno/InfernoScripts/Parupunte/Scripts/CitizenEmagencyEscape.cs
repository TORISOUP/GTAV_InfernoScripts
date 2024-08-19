using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("磯野～！緊急脱出しようぜ！")]
    [ParupunteIsono("きんきゅうだっしゅつ")]
    class CitizenEmagencyEscape : ParupunteScript
    {
        public CitizenEmagencyEscape(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Concat(new Ped[] {core.PlayerPed}).Where(
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
                EscapeVehicle(ped);
            }


            StartCoroutine(DelayFinished());
        }


        //車に乗ってたら緊急脱出する
        private void EscapeVehicle(Ped ped)
        {
            StartCoroutine(DelayParachute(ped));
        }

        private IEnumerable<object> DelayParachute(Ped ped)
        {
            ped.SetNotChaosPed(true);
            ped.ClearTasksImmediately();
            ped.Position += new Vector3(0, 0, 0.5f);
            ped.SetToRagdoll();
            yield return null;
            ped.ApplyForce(new Vector3(0, 0, 40.0f));

            ped.IsInvincible = true;
            yield return WaitForSeconds(1.5f);
            if(!ped.IsSafeExist()) yield break;
            ped.IsInvincible = false;
            ped.ParachuteTo(core.PlayerPed.Position);
            yield return WaitForSeconds(10);
            if (!ped.IsSafeExist()) yield break;
            ped.SetNotChaosPed(false);
        }


        IEnumerable<object> DelayFinished()
        {
            yield return WaitForSeconds(7);
            ParupunteEnd();
        }
    }
}
