using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("くるまついか")]
    class SpawnVheicle : ParupunteScript
    {
        private VehicleHash vehicleHash;
        private String _name;
        public SpawnVheicle(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {

        }

        public override void OnSetUp()
        {
            vehicleHash = Enum
                 .GetValues(typeof(VehicleHash))
                 .Cast<VehicleHash>()
                 .OrderBy(x => Random.Next())
                 .FirstOrDefault();

            var vehicleGxtEntry = Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, (int)vehicleHash);
            _name = Game.GetGXTEntry(vehicleGxtEntry);
        }

        public override void OnSetNames()
        {
            Name = $"{_name}生成";
        }

        public override void OnStart()
        {

            var v = GTA.World.CreateVehicle(new Model(vehicleHash), core.PlayerPed.Position + Vector3.WorldUp * 5.0f);
            if (v.IsSafeExist())
            {
                v.MarkAsNoLongerNeeded();
                v.FreezePosition = false;
                v.ApplyForce(Vector3.WorldUp * 5.0f);
                var p = v.CreateRandomPedAsDriver();
                if(p.IsSafeExist()) p.MarkAsNoLongerNeeded();
            }
            ParupunteEnd();

        }
    }
}
