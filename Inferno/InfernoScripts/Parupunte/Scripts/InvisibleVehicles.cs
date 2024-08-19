using System.Linq;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("光学迷彩(車両)")]
    internal class InvisibleVehicles : ParupunteScript
    {
        public InvisibleVehicles(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

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
