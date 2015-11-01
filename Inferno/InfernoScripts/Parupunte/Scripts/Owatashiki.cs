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
    class Owatashiki : ParupunteScript
    {
        public Owatashiki(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "オワタ式の可能性";

        public override void OnStart()
        {
            var player = core.PlayerPed;
            player.Health = 1;
            player.Armor = 0;
            ParupunteEnd();
        }
    }
}
