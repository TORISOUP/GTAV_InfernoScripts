using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("くるましゅうり")]
    internal class RepairVehicles : ParupunteScript
    {
        public RepairVehicles(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; }
            = new ParupunteConfigElement("車両修復", "");

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            SetVehicleFixed();
            ParupunteEnd();
        }

        private void SetVehicleFixed()
        {
            var radius = 100f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where
                (x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles)
            {
                vehicle.Repair();
            }

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.Repair();
            }
        }
    }
}
