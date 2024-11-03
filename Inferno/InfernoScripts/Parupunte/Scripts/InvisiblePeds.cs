using System.Linq;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("光学迷彩(仲間除くNPC)")]
    internal class InvisiblePeds : ParupunteScript
    {
        public InvisiblePeds(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            SetPedsInvisible();
            ParupunteEnd();
        }

        private void SetPedsInvisible()
        {
            var radius = 100.0f;
            var player = core.PlayerPed;
            var playerGroup = player.PedGroup;
            var peds = core.CachedPeds.Where(
                x => x.IsSafeExist() && !x.IsSameEntity(core.PlayerPed) && !x.IsCutsceneOnlyPed() &&
                     x.IsInRangeOf(player.Position, radius));

            foreach (var ped in peds)
            {
                if (playerGroup.Exists() && playerGroup.Contains(ped))
                {
                    continue;
                }

                var relationShip = ped.RelationshipGroup;
                if (relationShip == core.GetGTAObjectHashKey("PLAYER"))
                {
                    continue; //ミッション上での仲間は除外する(誤判定が起きる場合があるので暫定)
                }

                ped.IsVisible = false;
            }
        }
    }
}