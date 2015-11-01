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
    class KillCitizens : ParupunteScript
    {
        public KillCitizens(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "周辺市民全員即死";

        public override void OnStart()
        {
            KillCivilians();
            ParupunteEnd();
        }

        private void KillCivilians()
        {
            var radius = 100.0f;
            var player = core.PlayerPed;
            var peds = core.CachedPeds.Where(x => x.IsSafeExist()
                                               && !x.IsSameEntity(core.PlayerPed)
                                               && !x.IsRequiredForMission()
                                               && x.IsInRangeOf(player.Position, radius));

            foreach (var ped in peds)
            {
                ped.Kill();
            }
        }

    }
}
