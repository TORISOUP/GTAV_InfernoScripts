using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("磯野ー！空飛ぼうぜ！")]
    [ParupunteIsono("いその")]
    internal class Isono : ParupunteScript
    {
        public Isono(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

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

            var targetPositionInAri = core.PlayerPed.Position + new Vector3(0, 0, 500);

            var vehicleForcePower = 5;
            var pedForcePower = 10;

            player.CanRagdoll = true;
            player.SetToRagdoll(3000);
            player.IsCollisionProof = true;

            if (player.IsInVehicle()) player.CurrentVehicle.IsCollisionProof = true;

            foreach (var s in WaitForSeconds(10))
            {
                //一定以上打ち上がったらおわり
                if (player.Position.Z > targetPositionInAri.Z) break;

                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped)
                    {
                        var p = entity as Ped;
                        p.SetToRagdoll(3000);
                    }

                    var direction = (targetPositionInAri - entity.Position).Normalized();
                    var power = entity is Ped ? pedForcePower : vehicleForcePower;
                    entity.ApplyForce(direction * power, Vector3.RandomXYZ());
                }

                if (player.IsInVehicle() && player.CurrentVehicle.IsSafeExist())
                    player.CurrentVehicle.ApplyForce(Vector3.WorldUp * vehicleForcePower);
                else
                    player.ApplyForce(Vector3.WorldUp * pedForcePower);
                yield return null;
            }

            //着地するまで
            while (player.IsInVehicle() ? player.CurrentVehicle.IsInAir : player.IsInAir) yield return null;
            player.IsCollisionProof = false;
            if (player.IsInVehicle()) player.CurrentVehicle.IsCollisionProof = false;
            ParupunteEnd();
        }
    }
}