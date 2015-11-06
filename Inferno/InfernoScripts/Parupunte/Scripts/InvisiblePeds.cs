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
    class InvisiblePeds : ParupunteScript
    {
        public InvisiblePeds(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "光学迷彩(仲間除くNPC)";

        public override void OnStart()
        {
            SetPedsInvisible();
            ParupunteEnd();
        }

        private void SetPedsInvisible()
        {
            var radius = 100.0f;
            var player = core.PlayerPed;
            var playerGroup = player.CurrentPedGroup;
            var peds = core.CachedPeds.Where(
                x => x.IsSafeExist() && !x.IsSameEntity(core.PlayerPed) && !x.IsCutsceneOnlyPed() && x.IsInRangeOf(player.Position, radius));

            foreach (var ped in peds)
            {
                if (PedGroup.Exists(playerGroup) && playerGroup.Contains(ped)) { continue; }

                var relationShip = ped.RelationshipGroup;
                if (relationShip == core.GetGTAObjectHashKey("PLAYER")) { continue; }//ミッション上での仲間は除外する(誤判定が起きる場合があるので暫定)

                ped.IsVisible = false;
            }
        }
    }
}
