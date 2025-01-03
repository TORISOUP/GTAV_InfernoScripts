﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("くるまついか")]
    internal class SpawnVheicle : ParupunteScript
    {
        private string _name;
        private Ped _ped;
        private Vehicle _veh;
        private VehicleHash vehicleHash;

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
            _name = NativeFunctions.GetGXTEntry(vehicleGxtEntry);
        }

        public override void OnSetNames()
        {
            Name = $"{_name}生成";
        }

        public override void OnStart()
        {
            SpawnVehicle(ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            if (_veh.IsSafeExist())
            {
                _veh.MarkAsNoLongerNeeded();
            }
        }

        private async ValueTask SpawnVehicle(CancellationToken ct)
        {
            _veh = GTA.World.CreateVehicle(new Model(vehicleHash), core.PlayerPed.Position + Vector3.WorldUp * 5.0f);
            AutoReleaseOnParupunteEnd(_veh);
            if (_veh.IsSafeExist())
            {
                _veh.FreezePosition(false);
                _veh.ApplyForce(Vector3.WorldUp * 5.0f);
                _ped = _veh.CreateRandomPedAsDriver();
                //市民は勝手に消える状態にしないと自分で運転しない
                if (_ped.IsSafeExist())
                {
                    _ped.MarkAsNoLongerNeeded();
                    _ped.Task.DriveTo(_veh, core.PlayerPed.Position.Around(300), 10.0f, 100.0f,
                        DrivingStyle.AvoidTrafficExtremely);
                }

                //車がすぐに消えないように数秒待つ
                for (var i = 0; i < 20; i++)
                {
                    //車が画面に映ったらすぐに勝手に消える可能性はかなり低いので待機を終了する
                    if (!_veh.IsSafeExist() || _veh.IsOnScreen)
                    {
                        break;
                    }

                    await Delay100MsAsync(ct);
                }
            }

            ParupunteEnd();
        }
    }
}