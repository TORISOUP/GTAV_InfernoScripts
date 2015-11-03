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

        public override string Name => "今じゃあケツをふく紙にもなりゃしねってのによぉ!";

        public override void OnStart()
        {
            Game.Player.Money += 20000;
            ParupunteEnd();
        }
    }
}
