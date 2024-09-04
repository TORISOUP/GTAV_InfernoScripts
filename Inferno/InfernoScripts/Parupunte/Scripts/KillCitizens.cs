using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("周辺市民全員即死")]
    [ParupunteIsono("みんなばくはつ")]
    internal class KillCitizens : ParupunteScript
    {
        public KillCitizens(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            KillCitizensAsync(ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            ParupunteEnd();
        }

        private async ValueTask KillCitizensAsync(CancellationToken ct)
        {
            var radius = 100.0f;
            var player = core.PlayerPed;
            for (int l = 0; l < 3; l++)
            {
                var peds = core.CachedPeds.Where(x => x.IsSafeExist()
                                                      && !x.IsSameEntity(core.PlayerPed)
                                                      && !x.IsRequiredForMission()
                                                      && x.IsAlive
                                                      && x.IsInRangeOf(player.Position, radius))
                    .ToArray();

                for (int i = 0; i < peds.Length; i++)
                {
                    var ped = peds[i];
                    ped.Kill();
                    GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Rocket, 1.0f, 0.5f);
                    await Delay100MsAsync(ct);
                }
            }

            ParupunteEnd();
        }
    }
}