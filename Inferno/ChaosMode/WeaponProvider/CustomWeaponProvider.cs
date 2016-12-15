using System;
using System.Collections.Generic;
using System.Linq;

namespace Inferno.ChaosMode.WeaponProvider
{
    public class CustomWeaponProvider : IWeaponProvider
    {
        protected Weapon[] CustomShootWeapons;
        protected Weapon[] CustomClosedWeapons;
        protected Weapon[] CustomProjectileWeapons;
        protected Weapon[] CustomExcludeClosedWeapons;
        protected Weapon[] CustomDriveByWeapons;
        private readonly Random _random;

        /// <summary>
        /// 指定した武器リストからランダムに武器を提供する
        /// </summary>
        /// <param name="WeaponList">市民がドライブバイ以外で使用する武器</param>
        /// <param name="WeaponListForDriveBy">ドライブバイで使用する武器</param>
        public CustomWeaponProvider(IEnumerable<Weapon> weaponList, IEnumerable<Weapon> weaponListForDriveBy)
        {
            _random = new Random();
            //渡された武器リストから有効な武器のみにフィルタリング
            var weaponArray = weaponList as Weapon[] ?? weaponList.ToArray();
            CustomShootWeapons = weaponArray.Intersect(ChaosModeWeapons.ShootWeapons).ToArray();
            CustomClosedWeapons = weaponArray.Intersect(ChaosModeWeapons.ClosedWeapons).ToArray();
            CustomProjectileWeapons = weaponArray.Intersect(ChaosModeWeapons.ProjectileWeapons).ToArray();
            CustomDriveByWeapons = weaponListForDriveBy.Intersect(ChaosModeWeapons.DriveByWeapons).ToArray();
            CustomExcludeClosedWeapons = CustomShootWeapons.Concat(CustomProjectileWeapons).ToArray();
        }

        /// <summary>
        /// 射撃系の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomWeaponExcludeClosedWeapon()
        {
            return CustomExcludeClosedWeapons.Length > 0
                ? CustomExcludeClosedWeapons[_random.Next(0, CustomExcludeClosedWeapons.Length)]
                : Weapon.UNARMED;
        }

        /// <summary>
        /// ドライブバイ用の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomDriveByWeapon()
        {
            return CustomDriveByWeapons.Length > 0
                            ? CustomDriveByWeapons[_random.Next(0, CustomDriveByWeapons.Length)]
                            : Weapon.UNARMED;
        }
    }
}
