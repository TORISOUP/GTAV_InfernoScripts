using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class FixPedsAndVehicles : ParupunteScript
    {
        public FixPedsAndVehicles(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "周辺の人&車両回復";

        public override void OnStart()
        {
            RepairVehicles();
            RestoreCitizenHealthesAndArmors();
            ParupunteEnd();
        }

        private void RepairVehicles()
        {
            var radius = 100f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where
                (x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles)
            {
                vehicle.Repair();
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, vehicle, 0); //ドアロック解除
            }
        }

        private void RestoreCitizenHealthesAndArmors()
        {
            var radius = 100.0f;
            var player = core.PlayerPed;
            var peds = core.CachedPeds.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var ped in peds)
            {
                ped.Health = ped.MaxHealth;

                if(ped.Armor < 100)
                {
                    ped.Armor = 100;
                }
            }

            player.Health = player.MaxHealth;
            player.Armor = Game.Player.GetPlayerMaxArmor();

        }
    }
}
