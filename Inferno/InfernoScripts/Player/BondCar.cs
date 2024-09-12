using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Player
{
    internal class BondCar : InfernoScript
    {
        protected override void Setup()
        {
            config = LoadConfig<BondCarConfig>();


            OnTickAsObservable
                .Where(_ =>
                    PlayerPed.IsSafeExist() && PlayerPed.IsInVehicle() &&
                    Game.IsControlPressed(Control.VehicleAim) && Game.IsControlPressed(Control.VehicleAttack) &&
                    PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed
                )
                .ThrottleFirst(TimeSpan.FromMilliseconds(CoolDownMillSeconds), InfernoScheduler)
                .Subscribe(_ => Shoot());
        }

        private void Shoot()
        {
            var v = PlayerPed.CurrentVehicle;
            if (!v.IsSafeExist())
            {
                return;
            }

            //そこら辺の市民のせいにする
            var ped = CachedPeds.Where(x => x.IsSafeExist()).DefaultIfEmpty(PlayerPed).FirstOrDefault();
            InvincibleVehicleAsync(v, DestroyCancellationToken).Forget();
            CreateRpgBullet(v, ped, 1.5f);
            CreateRpgBullet(v, ped, -1.5f);
            v.EngineHealth *= 0.9f;
        }

        private void CreateRpgBullet(Vehicle vehicle, Ped ped, float rightOffset)
        {
            var startPosition = vehicle.GetOffsetFromEntityInWorldCoords(rightOffset, 0, 0.2f);
            var target = vehicle.GetOffsetFromEntityInWorldCoords(0, 1000, 0.2f);

            Function.Call(
                Hash.SHOOT_SINGLE_BULLET_BETWEEN_COORDS,
                startPosition.X,
                startPosition.Y,
                startPosition.Z,
                target.X,
                target.Y,
                target.Z,
                200,
                1,
                Weapon.VEHICLE_ROCKET,
                0,
                1,
                0,
                1000f);
        }


        private async ValueTask InvincibleVehicleAsync(Vehicle v, CancellationToken ct)
        {
            var lastValue = v.IsInvincible;
            v.IsInvincible = true;
            try
            {
                await DelaySecondsAsync(CoolDownMillSeconds / 1000f, ct);
            }
            finally
            {
                if (v.IsSafeExist())
                {
                    v.IsInvincible = lastValue;
                }
            }
        }

        #region config

        [Serializable]
        private class BondCarConfig : InfernoConfig
        {
            /// <summary>
            /// ミサイルの発射間隔[ms]
            /// </summary>
            public int CoolDownMillSeconds = 500;

            public override bool Validate()
            {
                return CoolDownMillSeconds >= 0;
            }
        }

        protected override string ConfigFileName { get; } = "BondCar.conf";
        private BondCarConfig config;
        private int CoolDownMillSeconds => config?.CoolDownMillSeconds ?? 500;

        #endregion
    }
}