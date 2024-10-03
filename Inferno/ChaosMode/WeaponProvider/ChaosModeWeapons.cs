using System;
using System.Linq;
using GTA;

namespace Inferno.ChaosMode.WeaponProvider
{
    /// <summary>
    /// 有効な武器一覧を管理する
    /// </summary>
    public static class ChaosModeWeapons
    {
        private static readonly Random random = new();

        static ChaosModeWeapons()
        {
            //射撃系の武器
            ShootWeapons = new[]
            {
                Weapon.AdvancedRifle,
                Weapon.APPistol,
                Weapon.AssaultRifle,
                Weapon.AssaultShotgun,
                Weapon.AssaultSMG,
                Weapon.BullpupShotgun,
                Weapon.BullpupRifle,
                Weapon.CarbineRifle,
                Weapon.CombatMG,
                Weapon.CombatPistol,
                Weapon.Exhaustion,
                Weapon.GrenadeLauncher,
                Weapon.GrenadeLauncherSmoke,
                Weapon.HeavySniper,
                Weapon.HeavyPistol,
                Weapon.HeavyShotgun,
                Weapon.FireExtinguisher,
                Weapon.Firework,
                Weapon.MG,
                Weapon.MicroSMG,
                Weapon.MiniGun,
                Weapon.Musket,
                Weapon.MarksmanRifle,
                Weapon.Pistol,
                Weapon.Pistol50,
                Weapon.PumpShotgun,
                Weapon.RPG,
                Weapon.SawedOffShotgun,
                Weapon.SMG,
                Weapon.SniperRifle,
                Weapon.StunGun,
                Weapon.PetrolCan,
                Weapon.RailGun,
                Weapon.FlareGun,
                Weapon.MarksmanPistol,
                Weapon.MachinePistol,
                Weapon.Gusenberg,
                Weapon.Revolver,
                Weapon.CompactRifle,
                Weapon.DoubleBarrelShotgun,
                Weapon.MiniSMG,
                Weapon.AutoShotgun,
                Weapon.CompactLauncher,
                Weapon.PistolMk2,
                Weapon.SMGMk2,
                Weapon.CombatMGMk2,
                Weapon.AssaultRifleMk2,
                Weapon.CarbineRifleMk2,
                Weapon.HeavySniperMk2,
                Weapon.HomingLauncher,
                Weapon.Widowmaker,
                Weapon.Raycarbine,
                Weapon.Raypistol,
            };

            // 爆発物系
            ExplosiveWeapons = new[]
            {
                Weapon.GrenadeLauncher,
                Weapon.RPG,
                Weapon.HomingLauncher,
                Weapon.Firework,
                Weapon.CompactLauncher,
                Weapon.Grenade,
                Weapon.Molotov,
                Weapon.StickyBomb,
                Weapon.ProximityMine,
                Weapon.PipeBomb,
                Weapon.BZGas,
                Weapon.FLARE,
                Weapon.SmokeGrenade,
                Weapon.PetrolCan,
                Weapon.RailGun
            };

            //近距離系
            ClosedWeapons = new[]
            {
                Weapon.BARBED_WIRE,
                Weapon.Bat,
                Weapon.Crowbar,
                Weapon.DROWNING,
                Weapon.Hammer,
                Weapon.GolfClub,
                Weapon.Knife,
                Weapon.NightStick,
                Weapon.Bottle,
                Weapon.Dagger,
                Weapon.Hatchet,
                Weapon.KnuckleDuster,
                Weapon.Machete,
                Weapon.Flashlight,
                Weapon.SwitchBlade,
                Weapon.Poolcue,
                Weapon.Wrench,
                Weapon.Battleaxe
            };

            //投げる系
            ProjectileWeapons = new[]
            {
                Weapon.Ball,
                Weapon.BZGas,
                Weapon.Grenade,
                Weapon.Molotov,
                Weapon.StickyBomb,
                Weapon.FLARE,
                Weapon.SmokeGrenade,
                Weapon.ProximityMine,
                Weapon.PipeBomb
            };

            //ドライブバイ
            DriveByWeapons = new[]
            {
                Weapon.Pistol,
                Weapon.APPistol,
                Weapon.CombatPistol,
                Weapon.HeavyPistol,
                Weapon.Pistol50,
                Weapon.FlareGun,
                Weapon.Revolver,
                Weapon.MicroSMG,
                Weapon.MachinePistol,
                Weapon.CompactRifle,
                Weapon.SawedOffShotgun,
                Weapon.DoubleBarrelShotgun,
                Weapon.StunGun,
                Weapon.MiniSMG,
                Weapon.AutoShotgun,
                Weapon.CompactLauncher,
                Weapon.PistolMk2,
                Weapon.SMGMk2
            };

            AllWeapons = ShootWeapons.Concat(ExplosiveWeapons)
                .Concat(ClosedWeapons)
                .Concat(ProjectileWeapons)
                .Distinct()
                .ToArray();

            ExcludeClosedWeapons = ShootWeapons.Concat(ProjectileWeapons).Distinct().ToArray();
        }

        public static Weapon[] ShootWeapons { get; }
        public static Weapon[] ClosedWeapons { get; }
        public static Weapon[] ProjectileWeapons { get; }
        public static Weapon[] ExcludeClosedWeapons { get; }
        public static Weapon[] DriveByWeapons { get; }
        public static Weapon[] AllWeapons { get; }

        public static Weapon[] ExplosiveWeapons { get; }

        public static Weapon GetRandomWeapon()
        {
            return AllWeapons[random.Next(0, AllWeapons.Length)];
        }

        public static bool IsPedEquippedWithMeleeWeapon(this Ped ped)
        {
            return ClosedWeapons.Contains((Weapon)ped.Weapons.Current.Hash);
        }
    }
}