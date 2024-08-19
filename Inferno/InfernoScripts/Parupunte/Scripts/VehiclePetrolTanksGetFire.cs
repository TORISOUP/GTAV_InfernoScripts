using System.Linq;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("周辺車両一斉発火")]
    internal class VehiclePetrolTanksGetFire : ParupunteScript
    {
        public VehiclePetrolTanksGetFire(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            GetVehiclesFire();
            ParupunteEnd();
        }

        private void GetVehiclesFire()
        {
            var radius = 100f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius));

            foreach (var vehicle in vehicles) vehicle.PetrolTankHealth = -1;

            if (player.IsInVehicle()) player.CurrentVehicle.PetrolTankHealth = -1;
        }
    }
}