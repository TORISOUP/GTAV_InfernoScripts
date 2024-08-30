using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Player
{
    internal class BondCar : InfernoScript
    {
        protected override void Setup()
        {
            config = LoadConfig<BondCarConfig>();

            OnThinnedTickAsObservable
                .Where(_ =>
                    PlayerVehicle.Value.IsSafeExist()
                    && this.IsGamePadPressed(GameKey.VehicleAim)
                    && this.IsGamePadPressed(GameKey.VehicleAttack)
                    && PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed
                )
                .ThrottleFirst(TimeSpan.FromMilliseconds(CoolDownMillSeconds), InfernoScheduler)
                .Subscribe(_ =>
                {
                    var v = PlayerVehicle.Value;
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
                });
        }

        private void CreateRpgBullet(Vehicle vehicle, Ped ped, float rightOffset)
        {
            var startPosition = vehicle.GetOffsetFromEntityInWorldCoords(rightOffset, 4, 0.2f);
            var target = vehicle.GetOffsetFromEntityInWorldCoords(0, 1000, 0.2f);

            NativeFunctions.ShootSingleBulletBetweenCoords(
                startPosition, target, 100, WeaponHash.RPG, ped, 500);
        }

        private async ValueTask InvincibleVehicleAsync(Vehicle v, CancellationToken ct)
        {
            var lastValue = v.IsInvincible;
            v.IsInvincible = true;
            try
            {
                await DelaySecondsAsync(2, ct);
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

        private class BondCarConfig : InfernoConfig
        {
            /// <summary>
            /// ミサイルの発射間隔[ms]
            /// </summary>
            public int CoolDownMillSeconds { get; } = 500;

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