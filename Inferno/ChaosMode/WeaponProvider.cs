using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.ChaosMode
{
    public class WeaponProvider
    {
        private Weapon[] shootWeapons;
        private Weapon[] closedWeapons;
        private Weapon[] projectileWeapons;
        private Weapon[] excludeClosedWeapons; //近接系武器以外
        private Weapon[] inVehicleWeapons; //ドライブバイ可能な武器
        private Weapon[] allWeapons;
        private Random random;
        public WeaponProvider()
        {
            random = new Random();
            //射撃系の武器
            shootWeapons = new[]
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
            closedWeapons = new[]
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
            projectileWeapons = new[]
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
            inVehicleWeapons = new[]
            {
                Weapon.PISTOL,
                Weapon.APPISTOL,
                Weapon.COMBATPISTOL,
                Weapon.MICROSMG,
                Weapon.SAWNOFFSHOTGUN,
                Weapon.STICKYBOMB
            };

            allWeapons = shootWeapons.Concat(closedWeapons).Concat(projectileWeapons).ToArray();
            excludeClosedWeapons = shootWeapons.Concat(projectileWeapons).ToArray();
        }

        /// <summary>
        /// ランダムに射撃系の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomShootWeapon()
        {
            return shootWeapons[random.Next(0, shootWeapons.Length)];
        }

        /// <summary>
        /// ランダムに近接武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomCloseWeapon()
        {
            return closedWeapons[random.Next(0, closedWeapons.Length)];
        }

        /// <summary>
        /// ランダムに投擲系の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomProjectileWeapon()
        {
            return projectileWeapons[random.Next(0, projectileWeapons.Length)];
        }

        /// <summary>
        /// すべての武器からランダムに取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomWeapon()
        {
            return allWeapons[random.Next(0,allWeapons.Length)];
        }

        /// <summary>
        /// 遠距離攻撃系の武器のみをランダムに取得
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomWeaponExcludeClosedWeapon()
        {
            return excludeClosedWeapons[random.Next(0, excludeClosedWeapons.Length)];
        }

        /// <summary>
        /// ドライブバイ用の武器
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomInVehicleWeapon()
        {
            return inVehicleWeapons[random.Next(0, inVehicleWeapons.Length)];
        }

        /// <summary>
        /// 射撃系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsShootWeapon(Weapon weapon)
        {
            return shootWeapons.Contains(weapon);
        }


        /// <summary>
        /// 近接系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsCloseWeapon(Weapon weapon)
        {
            return closedWeapons.Contains(weapon);
        }

        public bool IsProjectileWeapon(Weapon weapon)
        {
            return projectileWeapons.Contains(weapon);
        }
    }
}
