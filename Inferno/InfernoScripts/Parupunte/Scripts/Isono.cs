using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(true)]
    class Isono : ParupunteScript
    {
        public Isono(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "磯野ー！空飛ぼうぜ！";
        public override void OnStart()
        {
            StartCoroutine(IsonoCoroutine());
        }

        IEnumerable<object> IsonoCoroutine()
        {
            var player = core.PlayerPed;
            var entities =
                core.CachedVehicles
                    .Concat(core.CachedPeds.Cast<Entity>())
                    .Where(x => x.IsSafeExist() && x.IsInRangeOf(player.Position, 100));

            var upForce = new Vector3(0, 0, 8.0f);

            player.CanRagdoll = true;
            player.SetToRagdoll(3000);

            foreach (var s in WaitForSeconds(6))
            {
                foreach (var entity in entities.Where(x=>x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.SetToRagdoll(3000);
                    }
                    entity.ApplyForce(upForce);
                }
                if (player.IsInVehicle())
                {
                    player.CurrentVehicle.ApplyForce(upForce);
                }
                else
                {
                    player.ApplyForce(upForce/2.0f);
                }
                yield return null;
            }
            

            ParupunteEnd();

        } 
    }
}
