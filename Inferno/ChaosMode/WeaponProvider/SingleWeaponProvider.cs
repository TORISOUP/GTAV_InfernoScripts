﻿namespace Inferno.ChaosMode.WeaponProvider
{
    public class SingleWeaponProvider : IWeaponProvider
    {
        public SingleWeaponProvider(Weapon weapon)
        {
            current = weapon;
        }

        private Weapon current { get; }

        public Weapon GetRandomCloseWeapons()
        {
            return current;
        }

        public Weapon GetRandomAllWeapons()
        {
            return current;
        }

        public Weapon GetRandomWeaponExcludeClosedWeapon()
        {
            return current;
        }

        public Weapon GetRandomDriveByWeapon()
        {
            return current;
        }
    }
}