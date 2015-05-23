using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.ChaosMode.WeaponProvider
{
    /// <summary>
    /// 全ての武器リストから武器を取得する
    /// </summary>
    public class AllWeaponProvider : IWeaponProvider
    {
        protected Weapon[] ShootWeapons;
        protected Weapon[] ClosedWeapons;
        protected Weapon[] ProjectileWeapons;
        protected Weapon[] ExcludeClosedWeapons; //近接系武器以外
        protected Weapon[] InVehicleWeapons; //ドライブバイ可能な武器
        protected Weapon[] AllWeapons;
        protected Random Random;

        public AllWeaponProvider()
        {
            Random = new Random();
            //射撃系の武器
            ShootWeapons = new[]
            {
                Weapon.ADVANCEDRIFLE,
                Weapon.AIRSTRIKE_ROCKET,
                Weapon.APPISTOL,
                Weapon.ASSAULTRIFLE,
                Weapon.ASSAULTSHOTGUN,
                Weapon.ASSAULTSMG,
                Weapon.BLEEDING,
                Weapon.BULLPUPSHOTGUN,
                Weapon.CARBINERIFLE,
                Weapon.COMBATMG,
                Weapon.COMBATPISTOL,
                Weapon.COUGAR,
                Weapon.EXHAUSTION,
                Weapon.GRENADELAUNCHER,
                Weapon.GRENADELAUNCHER_SMOKE,
                Weapon.HEAVYSNIPER,
                Weapon.FIREEXTINGUISHER,
                Weapon.MG,
                Weapon.MICROSMG,
                Weapon.MINIGUN,
                Weapon.PISTOL,
                Weapon.PISTOL50,
                Weapon.PUMPSHOTGUN,
                Weapon.RPG,
                Weapon.SAWNOFFSHOTGUN,
                Weapon.SMG,
                Weapon.SNIPERRIFLE,
                Weapon.STINGER,
                Weapon.STUNGUN,
            };

            //近距離系
            ClosedWeapons = new[]
            {
                Weapon.BARBED_WIRE,
                Weapon.BAT,
                Weapon.CROWBAR,
                Weapon.DROWNING,
                Weapon.HAMMER,
                Weapon.GOLFCLUB,
                Weapon.KNIFE,
                Weapon.NIGHTSTICK,
                Weapon.PETROLCAN,
               
            };

            //投げる系
            ProjectileWeapons = new[]
            {
                Weapon.BALL,
                Weapon.BZGAS,
                Weapon.GRENADE,
                Weapon.MOLOTOV,
                Weapon.STICKYBOMB,
                Weapon.FLARE,
                Weapon.SMOKEGRENADE
            };

            //ドライブバイ
            InVehicleWeapons = new[]
            {
                Weapon.PISTOL,
                Weapon.APPISTOL,
                Weapon.COMBATPISTOL,
                Weapon.MICROSMG,
                Weapon.SAWNOFFSHOTGUN,
                Weapon.STUNGUN,
            };

            AllWeapons = ShootWeapons.Concat(ClosedWeapons).Concat(ProjectileWeapons).ToArray();
            ExcludeClosedWeapons = ShootWeapons.Concat(ProjectileWeapons).ToArray();
        }

        /// <summary>
        /// 遠距離攻撃系の武器のみをランダムに取得
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomWeaponExcludeClosedWeapon()
        {
            return ExcludeClosedWeapons[Random.Next(0, ExcludeClosedWeapons.Length)];
        }

        /// <summary>
        /// ドライブバイ用の武器
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomDriveByWeapon()
        {
            return InVehicleWeapons[Random.Next(0, InVehicleWeapons.Length)];
        }

        /// <summary>
        /// 射撃系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsShootWeapon(Weapon weapon)
        {
            return ShootWeapons.Contains(weapon);
        }


        /// <summary>
        /// 近接系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsCloseWeapon(Weapon weapon)
        {
            return ClosedWeapons.Contains(weapon);
        }

        public bool IsProjectileWeapon(Weapon weapon)
        {
            return ProjectileWeapons.Contains(weapon);
        }
    }
}
