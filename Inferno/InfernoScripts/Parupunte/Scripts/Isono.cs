using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("磯野ー！空飛ぼうぜ！")]
    [ParupunteIsono("いその")]
    internal class Isono : ParupunteScript
    {
        public Isono(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        bool _initPlayerCollisionProof;
        bool _initVehicleCollisionProof;
        private Dictionary<Entity, bool> _persistentEntities = new();

        public override void OnSetUp()
        {
            _initPlayerCollisionProof = core.PlayerPed.IsCollisionProof;
            _initVehicleCollisionProof = core.PlayerPed.IsInVehicle() && core.PlayerPed.CurrentVehicle.IsCollisionProof;
        }

        public override void OnStart()
        {
            IsonoAsync(ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            var player = core.PlayerPed;

            player.IsCollisionProof = _initPlayerCollisionProof;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = _initVehicleCollisionProof;
            }

            foreach (var pe in _persistentEntities)
            {
                if (pe.Key.IsSafeExist())
                {
                    pe.Key.IsPersistent = pe.Value;
                }
            }
        }

        private async ValueTask IsonoAsync(CancellationToken ct)
        {
            var player = core.PlayerPed;
            var entities =
                core.CachedVehicles
                    .Concat(core.CachedPeds.Cast<Entity>())
                    .Where(x => x.IsSafeExist() && x.IsInRangeOf(player.Position, 300))
                    .ToArray();

            var targetPositionInAri = core.PlayerPed.Position + new Vector3(0, 0, 500);

            var vehicleForcePower = 0.7f;
            var pedForcePower = 3;

            player.CanRagdoll = true;
            player.SetToRagdoll(3000);
            player.IsCollisionProof = true;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsCollisionProof = true;
            }

            foreach (var e in entities.Where(x => x.IsSafeExist()))
            {
                _persistentEntities[e] = e.IsPersistent;
                e.IsPersistent = true;
            }

            while (!ct.IsCancellationRequested)
            {
                var pZ = player.Position.Z;

                //一定以上打ち上がったらおわり
                if (pZ > targetPositionInAri.Z)
                {
                    break;
                }


                foreach (var entity in entities.Where(x => x.IsSafeExist()))
                {
                    if (entity is Ped p)
                    {
                        p.SetToRagdoll(3000);
                    }

                    var direction = (targetPositionInAri - entity.Position).Normalized();
                    var power = entity is Ped ? pedForcePower : vehicleForcePower;
                    if (entity.Position.Z < pZ)
                    {
                        entity.ApplyForce(direction * power, Vector3.RandomXYZ(), ForceType.MaxForceRot);
                    }
                }

                if (player.IsInVehicle() && player.CurrentVehicle.IsSafeExist())
                {
                    player.CurrentVehicle.ApplyForce(Vector3.WorldUp * vehicleForcePower);
                }
                else
                {
                    player.ApplyForce(Vector3.WorldUp * pedForcePower, Vector3.RandomXYZ(),
                        ForceType.MaxForceRot);
                }

                await YieldAsync(ct);
            }

            //着地するまで
            while (player.IsInVehicle() ? player.CurrentVehicle.IsInAir : player.IsInAir)
            {
                await YieldAsync(ct);
            }

            await DelaySecondsAsync(1, ct);

            ParupunteEnd();
        }
    }
}