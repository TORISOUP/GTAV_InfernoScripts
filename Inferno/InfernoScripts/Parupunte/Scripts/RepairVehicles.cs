using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA; using UniRx;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class RepairVehicles : ParupunteScript
    {
        public RepairVehicles(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "車両修復";

        public override void OnSetUp()
        {
            
        }

        public override void OnStart()
        {
            SetVehicleFixed();
            ParupunteEnd();
        }

        private void SetVehicleFixed()
        {
            var radius = 100f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where
                (x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles)
            {
                vehicle.Repair();
            }

            if(player.IsInVehicle())
            {
                player.CurrentVehicle.Repair();
            }
        }
    }
}
