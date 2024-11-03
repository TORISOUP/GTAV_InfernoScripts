using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// 爆雷
    /// </summary>
    internal class CitizenVehicleBomb : InfernoScript
    {
        private CitizenVehicleBombConfig _conf;

        protected override string ConfigFileName { get; } = "CitizenVehicleBomb.conf";
        private int Probability => _conf?.Probability ?? 10;

        protected override void Setup()
        {
            _conf = LoadConfig<CitizenVehicleBombConfig>();
            CreateInputKeywordAsObservable("CitizenVehicleBomb", "vbomb")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("VehicleBomb:" + IsActive);
                });

            IsActiveRP.Subscribe(x =>
            {

                if (x)
                {
                    CheckLoopAsync(ActivationCancellationToken).Forget();
                }
            });
            
        }

        private async ValueTask CheckLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsActive)
            {
                VehicleBombAction();
                await DelayAsync(TimeSpan.FromSeconds(_conf.IntervalSeconds), ct);
            }
        }
        
        private void VehicleBombAction()
        {
            //まだ発火していないプレイや以外のドライバのいるミッション対象外の車が対象
            var targetVehicles = CachedVehicles
                .Where(x =>
                    x.IsSafeExist() && x.IsAlive && x.PetrolTankHealth >= 0 && !x.IsPersistent && !x.IsPlayerVehicle()
                    && x.IsInRangeOf(PlayerPed.Position, _conf.Range)
                    && x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist());

            foreach (var vehicle in targetVehicles)
            {
                if (Random.Next(0, 100) <= Probability)
                {
                    vehicle.PetrolTankHealth = -1;
                }
            }
        }

        [Serializable]
        private class CitizenVehicleBombConfig : InfernoConfig
        {
            private int _probability = 10;
            private int _intervalSeconds = 5;
            private int _range = 100;

            public int Range
            {
                get => _range;
                set => _range = value.Clamp(10, 1000);
            }

            public int Probability
            {
                get => _probability;
                set => _probability = value.Clamp(0, 100);
            }

            public int IntervalSeconds
            {
                get => _intervalSeconds;
                set => _intervalSeconds = value.Clamp(1, 60);
            }

            public override bool Validate()
            {
                return Probability is > 0 and <= 100;
            }
        }
        
        
        #region UI

        public override bool UseUI => true;
        public override string DisplayName => EntitiesLocalize.CitizenVehicleBombTitle;

        public override string Description => EntitiesLocalize.CitizenVehicleBombDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Entities;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            subMenu.AddSlider(
                $"Range: {_conf.Range}[m]",
                EntitiesLocalize.CitizenVehicleBombRange,
                _conf.Range,
                1000,
                x =>
                {
                    x.Value = _conf.Range;
                    x.Multiplier = 10;
                }, item =>
                {
                    _conf.Range = item.Value;
                    item.Title = $"Range: {_conf.Range}[m]";
                });

            subMenu.AddSlider(
                $"Interval: {_conf.IntervalSeconds}[s]",
                EntitiesLocalize.CitizenVehicleBombInterval,
                _conf.IntervalSeconds,
                60,
                x =>
                {
                    x.Value = _conf.IntervalSeconds;
                    x.Multiplier = 1;
                }, item =>
                {
                    _conf.IntervalSeconds = item.Value;
                    item.Title = $"Interval: {_conf.IntervalSeconds}[s]";
                });

            subMenu.AddSlider(
                $"Probability: {_conf.IntervalSeconds}[%]",
                EntitiesLocalize.CitizenVehicleBombProbability,
                _conf.IntervalSeconds,
                100,
                x =>
                {
                    x.Value = _conf.Probability;
                    x.Multiplier = 1;
                }, item =>
                {
                    _conf.Probability = item.Value;
                    item.Title = $"Probability: {_conf.Probability}[%]";
                });

            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                _conf = LoadDefaultConfig<CitizenVehicleBombConfig>();
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(_conf);
                DrawText($"Saved to {ConfigFileName}");
            });
        }

        #endregion
    }
}