using GTA.Math;
using GTA.Native;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class VehicleSpeedUp : ParupunteScript
    {
        public VehicleSpeedUp(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "エンジンパワーアップ";

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            foreach (var v in core.CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsAlive))
            {
                v.EnginePowerMultiplier = 200.0f;
                v.EngineTorqueMultiplier = 200.0f;
            }

            ParupunteEnd();
        }
    }
}
