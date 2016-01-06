using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class InvisibleVehicles : ParupunteScript
    {
        public InvisibleVehicles(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "光学迷彩(車両)";

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            SetVehiclesInvisible();
            ParupunteEnd();
        }

        private void SetVehiclesInvisible()
        {
            var radius = 100f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where
                (x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles)
            {
                vehicle.IsVisible = false;
            }

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.IsVisible = false;
                player.IsVisible = true;
            }
        }
    }
}
