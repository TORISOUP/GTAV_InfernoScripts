using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class ClearWantedLevel : ParupunteScript
    {
        public ClearWantedLevel(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "無罪放免";

        public override void OnStart()
        {
            Game.Player.WantedLevel = 0;
            ParupunteEnd();
        }
    }
}
