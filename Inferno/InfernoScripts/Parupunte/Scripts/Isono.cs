using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug]
    internal class Isono : ParupunteScript
    {
        public Isono(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "磯野ー！空飛ぼうぜ！";

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            StartCoroutine(IsonoCoroutine());
        }

        private IEnumerable<object> IsonoCoroutine()
        {
            var player = core.PlayerPed;
            var entities =
                core.CachedVehicles
                    .Concat(core.CachedPeds.Cast<Entity>())
                    .Where(x => x.IsSafeExist() && x.IsInRangeOf(player.Position, 100));

            var upForce = new Vector3(0, 0, 8.0f);

            player.CanRagdoll = true;
            player.SetToRagdoll(3000);
            player.IsCollisionProof = true;
            if (player.IsInvincible)
            {
                player.CurrentVehicle.IsCollisionProof = true;
            }

            var randomVector = Utilities.InfernoUtilities.CreateRandomVector();

            //6秒間空に打ち上げる
            foreach (var s in WaitForSeconds(6))
            {
                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.SetToRagdoll(3000);
                    }
                    entity.ApplyForce(upForce, randomVector);
                }
                if (player.IsInVehicle() && player.CurrentVehicle.IsSafeExist())
                {
                    player.CurrentVehicle.ApplyForce(upForce);
                }
                else
                {
                    player.ApplyForce(upForce, randomVector);
                }
                yield return null;
            }

            while (player.IsInVehicle() ? player.CurrentVehicle.IsInAir : player.IsInAir)
            {
                yield return null;
            }
            player.IsCollisionProof = false;
            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = false;
            }
            ParupunteEnd();
        }
    }
}
