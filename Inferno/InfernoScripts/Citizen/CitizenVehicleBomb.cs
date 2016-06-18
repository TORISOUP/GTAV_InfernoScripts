using System;
using System.Linq;
using Inferno.Utilities;
using UniRx;

namespace Inferno
{

    /// <summary>
    /// 爆雷
    /// </summary>
    internal class CitizenVehicleBomb : InfernoScript
    {
        class CitizenVehicleBombConfig : InfernoConfig
        {
            public int Probability { get; set; } = 10;

            public override bool Validate()
            {
                return Probability > 0 && Probability <= 100;
            }
        }

        protected override string ConfigFileName { get; } = "CitizenVehicleBomb.conf";
        private CitizenVehicleBombConfig config;
        private int Probability => config?.Probability ?? 10;

        protected override void Setup()
        {
            config = LoadConfig<CitizenVehicleBombConfig>();
            CreateInputKeywordAsObservable("vbomb")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("VehicleBomb:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            CreateTickAsObservable(5000)
                .Where(_ => IsActive)
                .Subscribe(_ => VehicleBombAction());
        }

        private void VehicleBombAction()
        {
            //まだ発火していないプレイや以外のドライバのいるミッション対象外の車が対象
            var targetVehicles = CachedVehicles
                .Where(x =>
                    x.IsSafeExist() && x.IsAlive && x.PetrolTankHealth >= 0 && !x.IsPersistent && !x.IsPlayerVehicle()
                    && x.GetPedOnSeat(GTA.VehicleSeat.Driver).IsSafeExist());

            foreach (var vehicle in targetVehicles)
            {
                if (Random.Next(0, 100) <= Probability)
                {
                    vehicle.PetrolTankHealth = -1;
                }
            }
        }
    }
}
