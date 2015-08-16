using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{
    /// <summary>
    /// 爆雷
    /// </summary>
    class CitizenVehicleBomb:InfernoScript
    {
        protected override int TickInterval => 5000;
        private float probability = 10;

        protected override void Setup()
        {

            CreateInputKeywordAsObservable("vbomb")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("VehicleBomb:"+IsActive,3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            OnTickAsObservable
                .Where(_ => IsActive)
                .Subscribe(_ =>VehicleBombAction());
        }

        void VehicleBombAction()
        {
            //まだ発火していないプレイや以外のドライバのいるミッション対象外の車が対象
            var targetVehicles = CachedVehicles
                .Where(x =>
                    x.IsSafeExist() && x.IsAlive && x.PetrolTankHealth >= 0 && !x.IsPersistent && !x.IsPlayerVehicle()
                    && x.GetPedOnSeat(GTA.VehicleSeat.Driver).IsSafeExist());

            foreach (var vehicle in targetVehicles)
            {
                if (Random.Next(0, 100) <= probability)
                {
                    vehicle.PetrolTankHealth = -1;
                }
            }
        }
    }
}
