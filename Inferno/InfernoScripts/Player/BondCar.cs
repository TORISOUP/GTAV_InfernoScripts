using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;

namespace Inferno.InfernoScripts.Player
{
    class BondCar : InfernoScript
    {

        protected override void Setup()
        {
            var isRight = true;

            OnTickAsObservable
                .Where(_ =>
                    PlayerVehicle.Value.IsSafeExist()
                    && this.IsGamePadPressed(GameKey.VehicleAim)
                    && this.IsGamePadPressed(GameKey.VehicleAttack)
                    && PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed
                 )
                .ThrottleFirst(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ =>
                {
                    var v = PlayerVehicle.Value;
                    //そこら辺の市民のせいにする
                    var ped = CachedPeds.Where(x => x.IsSafeExist()).DefaultIfEmpty(PlayerPed).FirstOrDefault();
                    StartCoroutine(InvincibleVehicle(v, 2));
                    CreateRpgBullet(v, ped, isRight ? 1.5f : -1.5f);
                    //左右交互に
                    isRight = !isRight;
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
