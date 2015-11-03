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
    class RestorePlayerHealthAndArmor : ParupunteScript
    {
        public RestorePlayerHealthAndArmor(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "全回復";

        public override void OnStart()
        {
            var player = core.PlayerPed;

            player.Health = player.MaxHealth;
            player.Armor = Game.Player.GetPlayerMaxArmor();

            ParupunteEnd();
        }
    }
}
