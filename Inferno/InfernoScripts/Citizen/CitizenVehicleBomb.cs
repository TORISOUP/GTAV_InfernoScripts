using System;
using System.Linq;
using System.Reactive.Linq;
using GTA;
using Inferno.Utilities;

namespace Inferno
{
    /// <summary>
    /// 爆雷
    /// </summary>
    internal class CitizenVehicleBomb : InfernoScript
    {
        private CitizenVehicleBombConfig config;

        protected override string ConfigFileName { get; } = "CitizenVehicleBomb.conf";
        private int Probability => config?.Probability ?? 10;

        protected override void Setup()
        {
            config = LoadConfig<CitizenVehicleBombConfig>();
            CreateInputKeywordAsObservable("vbomb")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("VehicleBomb:" + IsActive);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            CreateTickAsObservable(TimeSpan.FromSeconds(5))
                .Where(_ => IsActive)
                .Subscribe(_ => VehicleBombAction());
        }

        private void VehicleBombAction()
        {
            //まだ発火していないプレイや以外のドライバのいるミッション対象外の車が対象
            var targetVehicles = CachedVehicles
                .Where(x =>
                    x.IsSafeExist() && x.IsAlive && x.PetrolTankHealth >= 0 && !x.IsPersistent && !x.IsPlayerVehicle()
                    && x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist());

            foreach (var vehicle in targetVehicles)
            {
                if (Random.Next(0, 100) <= Probability)
                {
                    vehicle.PetrolTankHealth = -1;
                }
            }
        }

        private class CitizenVehicleBombConfig : InfernoConfig
        {
            public int Probability { get; } = 10;

            public override bool Validate()
            {
                return Probability > 0 && Probability <= 100;
            }
        }
    }
}