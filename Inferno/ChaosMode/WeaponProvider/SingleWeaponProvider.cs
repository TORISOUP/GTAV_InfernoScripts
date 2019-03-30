using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.ChaosMode.WeaponProvider
{
    public class SingleWeaponProvider : IWeaponProvider
    {
        private Weapon current { get; set; }

        public SingleWeaponProvider(Weapon weapon)
        {
            current = weapon;
        }

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
