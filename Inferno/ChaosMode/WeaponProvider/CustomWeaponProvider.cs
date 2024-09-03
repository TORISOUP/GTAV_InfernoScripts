using System;
using System.Collections.Generic;
using System.Linq;

namespace Inferno.ChaosMode.WeaponProvider
{
    public class CustomWeaponProvider : IWeaponProvider
    {
        private readonly Random _random;
        protected Weapon[] AllWeapons;
        protected Weapon[] CustomClosedWeapons;
        protected Weapon[] CustomDriveByWeapons;
        protected Weapon[] CustomExcludeClosedWeapons;
        protected Weapon[] CustomProjectileWeapons;
        protected Weapon[] CustomShootWeapons;
        protected Weapon[] CustomExplosiveWeapons;

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
            AllWeapons = weaponArray;
            CustomShootWeapons = weaponArray.Intersect(ChaosModeWeapons.ShootWeapons).ToArray();
            CustomClosedWeapons = weaponArray.Intersect(ChaosModeWeapons.ClosedWeapons).ToArray();
            CustomProjectileWeapons = weaponArray.Intersect(ChaosModeWeapons.ProjectileWeapons).ToArray();
            CustomDriveByWeapons = weaponListForDriveBy.Intersect(ChaosModeWeapons.DriveByWeapons).ToArray();
            CustomExplosiveWeapons = weaponArray.Intersect(ChaosModeWeapons.ExplosiveWeapons).ToArray();
            CustomExcludeClosedWeapons = CustomShootWeapons.Concat(CustomProjectileWeapons)
                .Concat(CustomClosedWeapons)
                .ToArray();
            
        }

        public Weapon GetRandomCloseWeapons()
        {
            return CustomClosedWeapons.Length == 0
                ? Weapon.Unarmed
                : CustomClosedWeapons[_random.Next(0, CustomClosedWeapons.Length)];
        }

        public Weapon GetRandomAllWeapons()
        {
            return AllWeapons.Length == 0 ? Weapon.Unarmed : AllWeapons[_random.Next(0, AllWeapons.Length)];
        }

        /// <summary>
        /// 射撃系の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomWeaponExcludeClosedWeapon()
        {
            return CustomExcludeClosedWeapons.Length > 0
                ? CustomExcludeClosedWeapons[_random.Next(0, CustomExcludeClosedWeapons.Length)]
                : Weapon.Unarmed;
        }

        public Weapon GetExplosiveWeapon()
        {
            return CustomExplosiveWeapons.Length > 0
                ? CustomExplosiveWeapons[_random.Next(0, CustomExplosiveWeapons.Length)]
                : Weapon.Unarmed;
        }

        /// <summary>
        /// ドライブバイ用の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomDriveByWeapon()
        {
            return CustomDriveByWeapons.Length > 0
                ? CustomDriveByWeapons[_random.Next(0, CustomDriveByWeapons.Length)]
                : Weapon.Unarmed;
        }
    }
}