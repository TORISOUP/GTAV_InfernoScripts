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
    class AddPlayerMoney : ParupunteScript
    {
        public AddPlayerMoney(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "使い道は入院費";

        public override void OnStart()
        {
            Game.Player.Money += 2000;
            ParupunteEnd();
        }
    }
}
