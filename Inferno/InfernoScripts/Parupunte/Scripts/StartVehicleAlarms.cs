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
    class StartVehicleAlarms : ParupunteScript
    {
        public StartVehicleAlarms(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "車のアラームが作動しちまった！";

        public override void OnStart()
        {
            StartCarAlarms();
            ParupunteEnd();
        }

        private void StartCarAlarms()
        {
            var radius = 500f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles)
            {
                vehicle.HasAlarm = true;
                vehicle.StartAlarm();
            }

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.HasAlarm = true;
                player.CurrentVehicle.StartAlarm();
            }
        }

    }
}
