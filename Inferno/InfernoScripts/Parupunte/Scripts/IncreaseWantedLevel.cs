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
    class IncreaseWantedLevel : ParupunteScript
    {
        public IncreaseWantedLevel(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "日頃の行いが悪い";

        public override void OnStart()
        {
            IncreasePlayerWantedLevel();
            ParupunteEnd();
        }

        private void IncreasePlayerWantedLevel()
        {
            var playerChar = Game.Player;
            var MaxWantedLevel = Game.MaxWantedLevel;

            if (MaxWantedLevel < playerChar.WantedLevel + 4)
            {
                playerChar.WantedLevel = MaxWantedLevel;
            }
            else
            {
                playerChar.WantedLevel = playerChar.WantedLevel + 4;
            }
        }
    }
}
