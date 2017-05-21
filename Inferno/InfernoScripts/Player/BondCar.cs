using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;
using UniRx;

namespace Inferno.InfernoScripts.Player
{
    class BondCar : InfernoScript
    {
        #region config
        class BondCarConfig : InfernoConfig
        {
            /// <summary>
            /// ミサイルの発射間隔[ms]
            /// </summary>
            public int CoolDownMillSeconds { get; set; } = 500;

            public override bool Validate()
            {
                return CoolDownMillSeconds >= 0;
            }
        }

        protected override string ConfigFileName { get; } = "BondCar.conf";
        private BondCarConfig config;
        private int CoolDownMillSeconds => config?.CoolDownMillSeconds ?? 500;

        #endregion

        protected override void Setup()
        {
            config = LoadConfig<BondCarConfig>();

            OnTickAsObservable
                .Where(_ =>
                    PlayerVehicle.Value.IsSafeExist()
                    && this.IsGamePadPressed(GameKey.VehicleAim)
                    && this.IsGamePadPressed(GameKey.VehicleAttack)
                    && PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed
                 )
                .ThrottleFirst(TimeSpan.FromMilliseconds(CoolDownMillSeconds), InfernoScriptScheduler)
                .Subscribe(_ =>
                {
                    var v = PlayerVehicle.Value;
                    //そこら辺の市民のせいにする
                    var ped = CachedPeds.Where(x => x.IsSafeExist()).DefaultIfEmpty(PlayerPed).FirstOrDefault();
                    StartCoroutine(InvincibleVehicle(v, 2));
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

        private IEnumerable<object> InvincibleVehicle(Vehicle v, float sec)
        {

            foreach (var c in WaitForSeconds(sec))
            {
                if (v.IsSafeExist())
                {
                    v.IsInvincible = true;
                }
                yield return null;
            }

            if (!v.IsSafeExist()) yield break;
            v.IsInvincible = false;
        }
    }
}
