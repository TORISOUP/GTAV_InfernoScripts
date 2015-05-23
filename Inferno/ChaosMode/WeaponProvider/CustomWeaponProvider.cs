using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.ChaosMode.WeaponProvider
{
    public class CustomWeaponProvider: IWeaponProvider
    {
        #region AllWeapons
        public Weapon[] AllExcludeClosedWeapons { get; set; }
        public Weapon[] AllDriveByWeapons { get; set; }
        public Weapon[] AllProjectileWeapons { get; set; }
        public Weapon[] AllClosedWeapons { get; set; }
        public Weapon[] AllShootWeapons { get; set; }


        protected void SetUpWeapons()
        {
            AllShootWeapons = new[]
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
            AllClosedWeapons = new[]
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
            AllProjectileWeapons = new[]
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
            AllDriveByWeapons = new[]
            {
                Weapon.PISTOL,
                Weapon.APPISTOL,
                Weapon.COMBATPISTOL,
                Weapon.MICROSMG,
                Weapon.SAWNOFFSHOTGUN,
                Weapon.STUNGUN,
            };

            AllExcludeClosedWeapons = AllShootWeapons.Concat(AllProjectileWeapons).ToArray();
        }

        #endregion

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

            //全ての有効な武器一覧を設定する
            SetUpWeapons();

            //渡された武器リストから有効な武器のみにフィルタリング
            var weaponArray = weaponList as Weapon[] ?? weaponList.ToArray();
            CustomShootWeapons = weaponArray.Intersect(AllShootWeapons).ToArray();
            CustomClosedWeapons = weaponArray.Intersect(AllClosedWeapons).ToArray();
            CustomProjectileWeapons = weaponArray.Intersect(AllProjectileWeapons).ToArray();
            CustomDriveByWeapons = weaponListForDriveBy.Intersect(AllDriveByWeapons).ToArray();
            CustomExcludeClosedWeapons = CustomShootWeapons.Concat(CustomProjectileWeapons).ToArray();
        }

        /// <summary>
        /// 射撃系の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomWeaponExcludeClosedWeapon()
        {
            return CustomExcludeClosedWeapons[_random.Next(0, CustomExcludeClosedWeapons.Length)];
        }

        /// <summary>
        /// ドライブバイ用の武器を取得する
        /// </summary>
        /// <returns></returns>
        public Weapon GetRandomDriveByWeapon()
        {
            return CustomDriveByWeapons[_random.Next(0, CustomDriveByWeapons.Length)];
        }

        /// <summary>
        /// 射撃系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsShootWeapon(Weapon weapon)
        {
            return AllShootWeapons.Contains(weapon);
        }


        /// <summary>
        /// 近接系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsCloseWeapon(Weapon weapon)
        {
            return AllClosedWeapons.Contains(weapon);
        }

        /// <summary>
        /// 投擲系の武器であるか
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsProjectileWeapon(Weapon weapon)
        {
            return AllProjectileWeapons.Contains(weapon);
        }
    }
}
