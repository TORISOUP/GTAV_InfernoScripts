using GTA;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("車両パワーうｐ")]
    internal class StrongVehicles : ParupunteScript
    {
        public StrongVehicles(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            SetVehiclesStrong();
            ParupunteEnd();
        }

        private void SetVehiclesStrong()
        {
            var range = 100.0f;
            var player = core.PlayerPed;
            var vehicles = core.CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(player.Position, range));

            foreach (var vehicle in vehicles)
            {
                EnhanceVehicleDurability(vehicle);
            }

            if (player.IsInVehicle())
            {
                EnhanceVehicleDurability(player.CurrentVehicle);
            }
        }

        private void EnhanceVehicleDurability(Vehicle vehicle)
        {
            var bulletProof = vehicle.IsBulletProof;
            var fireProof = vehicle.IsFireProof;
            var collisionProof = vehicle.IsCollisionProof;
            var meleeProof = vehicle.IsMeleeProof;

            //isExplosionProofは2.4時点では機能していない
            vehicle.SetProofs(bulletProof, fireProof, true, collisionProof, meleeProof, false, false, false);

            vehicle.BodyHealth = 3000;
            vehicle.EngineHealth = 3000;
            vehicle.PetrolTankHealth = 3000;
        }
    }
}
