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
    class InvisibleVehicles : ParupunteScript
    {
        public InvisibleVehicles(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "光学迷彩(車両)";

        public override void OnSetUp()
        {
            
        }

        public override void OnStart()
        {
            SetVehiclesInvisible();
            ParupunteEnd();
        }

        private void SetVehiclesInvisible()
        {
            var radius = 100f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where
                (x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles)
            {
                vehicle.IsVisible = false;
            }

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsVisible = false;
            }
        }
    }
}
