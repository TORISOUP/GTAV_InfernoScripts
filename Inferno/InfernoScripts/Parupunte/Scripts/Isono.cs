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

        bool _initPlayerInvincible;
        bool _initVehicleInvincible;
        private Dictionary<Entity, bool> _persistentEntities = new();

        public override void OnSetUp()
        {
            _initPlayerInvincible = core.PlayerPed.IsInvincible;
            _initVehicleInvincible = core.PlayerPed.IsInVehicle() && core.PlayerPed.CurrentVehicle.IsInvincible;
        }

        public override void OnStart()
        {
            IsonoAsync(ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            var player = core.PlayerPed;

            player.IsInvincible = _initPlayerInvincible;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsInvincible = _initVehicleInvincible;
            }

            foreach (var pe in _persistentEntities)
            {
                if (pe.Key.IsSafeExist() && pe.Key.IsPersistent)
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
            player.IsInvincible = true;

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsInvincible = true;
            }

            foreach (var e in entities.Where(x => x.IsSafeExist()))
            {
                _persistentEntities[e] = e.IsPersistent;
                e.IsPersistent = true;
            }

            // Up
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
            bool IsInAir()
            {
                if (player.IsInVehicle())
                {
                    return player.CurrentVehicle.IsInAir || player.CurrentVehicle.Velocity.Length() > 10f;
                }

                return player.IsInAir || player.Velocity.Length() > 10f;
            }


            var targetEntities = entities.Where(x => x.IsSafeExist() && x != player && player.CurrentVehicle != x).ToArray();

            while (IsInAir())
            {
                // Playerに向かって飛んでくる
                foreach (var entity in targetEntities)
                {
                    var targetPosition = Vector3.Lerp(player.Position, entity.Position, 0.2f);
                    var vec = (targetPosition - entity.Position) + Vector3.WorldDown * 20;
                    var direction = vec.Normalized();
                    var length = vec.Length();
                    var power = entity is Ped ? pedForcePower : vehicleForcePower;
                    entity.ApplyForce(direction * power * length * 0.025f, Vector3.RandomXYZ(), ForceType.MaxForceRot);
                }

                await YieldAsync(ct);
            }

            await DelaySecondsAsync(1, ct);

            // ぜんぶ吹っ飛ばす
            foreach (var entity in targetEntities)
            {
                var vec = -(player.Position - entity.Position);
                var direction = (vec.Normalized() + Vector3.WorldUp * 5).Normalized;
                var power = entity is Ped ? pedForcePower : vehicleForcePower;
                entity.ApplyForce(direction * power * 50, Vector3.RandomXYZ(), ForceType.MaxForceRot);
            }

            GTA.World.AddExplosion(player.Position, GTA.ExplosionType.Grenade, 100, 1, null, true, false);

            await DelaySecondsAsync(1, ct);


            ParupunteEnd();
        }
    }
}