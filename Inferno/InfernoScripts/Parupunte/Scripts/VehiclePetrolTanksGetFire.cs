using System.Linq;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class VehiclePetrolTanksGetFire : ParupunteScript
    {
        public VehiclePetrolTanksGetFire(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "周辺車両一斉発火";

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

            foreach (var vehicle in vehicles)
            {
                vehicle.PetrolTankHealth = -1;
            }

            if (player.IsInVehicle())
            {
                player.CurrentVehicle.PetrolTankHealth = -1;
            }
        }
    }
}
