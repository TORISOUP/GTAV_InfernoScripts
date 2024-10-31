using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// 市ニトロ
    /// </summary>
    public class CitizenNitro : InfernoScript
    {
        private readonly int[] _velocities = { -100, -70, -50, 50, 70, 100 };

        private readonly string Keyword = "cnitro";
        private CitizenNitroConfig config;

        protected override string ConfigFileName { get; } = "CitizenNitrous.conf";
        private int Probability => config?.Probability ?? 7;

        protected override void Setup()
        {
            config = LoadConfig<CitizenNitroConfig>();

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable("CitizenNitrous", Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenNitrous:" + IsActive);
                });

            IsActiveRP.Subscribe(x =>
            {
                if (x)
                {
                    LoopAsync(ActivationCancellationToken).Forget();
                }
            });
        }

        private async ValueTask LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsActive)
            {
                CitizenNitroAction();
                await DelaySecondsAsync(config.IntervalSeconds, ct);
            }
        }


        /// <summary>
        /// ニトロ対象の選別
        /// </summary>
        private void CitizenNitroAction()
        {
            try
            {
                var playerVehicle = this.GetPlayerVehicle();

                var nitroAvailableVehs = CachedVehicles
                    .Where(x => (!playerVehicle.IsSafeExist() || x != playerVehicle) &&
                                x.IsInRangeOfIgnoreZ(PlayerPed.Position, config.Range) &&
                                x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist() && !x.IsPersistent);

                foreach (var veh in nitroAvailableVehs)
                {
                    if (Random.Next(0, 100) <= Probability)
                    {
                        DelayCoroutine(veh, ActivationCancellationToken).Forget();
                    }
                }
            }
            catch (Exception e)
            {
                //nice catch!
                LogWrite(e.ToString());
            }
        }

        /// <summary>
        /// ニトロの発動を遅延させる
        /// </summary>
        private async ValueTask DelayCoroutine(Vehicle v, CancellationToken ct)
        {
            var waitSeconds = Random.NextDouble() * 5.0f;
            await DelayAsync(TimeSpan.FromSeconds(waitSeconds), ct);
            var driver = v.GetPedOnSeat(VehicleSeat.Driver);
            
            // たまに緊急脱出
            if (Random.Next(0, 100) < 30)
            {
                EscapeVehicle(driver);
            }

            NitroVehicle(v);
        }

        /// <summary>
        /// 車をニトロする
        /// </summary>
        /// <param name="vehicle"></param>
        private void NitroVehicle(Vehicle vehicle)
        {
            if (!vehicle.IsSafeExist())
            {
                return;
            }

            var velocitiesSpeed = _velocities[Random.Next(0, _velocities.Length)];

            if (velocitiesSpeed > 0 && Random.Next(0, 100) <= 15)
            {
                vehicle.Quaternion = Quaternion.RotationAxis(vehicle.RightVector, Random.Next(20, 60) / 100.0f) *
                                     vehicle.Quaternion;
            }

            vehicle.SetForwardSpeed(vehicle.Speed + velocitiesSpeed);

            // プレイヤーの近くならエフェクトを出す
            if (vehicle.IsInRangeOfIgnoreZ(PlayerPed.Position, 50))
            {

                Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
                {
                    vehicle.Position.X,
                    vehicle.Position.Y,
                    vehicle.Position.Z,
                    -1,
                    0.0f,
                    true,
                    false,
                    0.1f
                });
            }
        }


        //車に乗ってたら緊急脱出する
        private void EscapeVehicle(Ped ped)
        {
            if (!ped.IsSafeExist())
            {
                return;
            }

            DelayParachute(ped, ActivationCancellationToken).Forget();
        }

        private async ValueTask DelayParachute(Ped ped, CancellationToken ct)
        {
            ped.SetNotChaosPed(true);
            ped.ClearTasksImmediately();
            ped.Position += new Vector3(0, 0, 0.5f);
            ped.SetToRagdoll();

            await DelayAsync(TimeSpan.FromSeconds(0.1f), ct);

            ped.ApplyForce(new Vector3(0, 0, 60.0f));
            ped.IsInvincible = true;

            try
            {
                await DelayAsync(TimeSpan.FromSeconds(1.5f), ct);
                if (!ped.IsSafeExist())
                {
                    return;
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    ped.IsInvincible = false;
                }
            }

            ped.ParachuteTo(PlayerPed.Position);

            await DelayAsync(TimeSpan.FromSeconds(15), ct);

            if (!ped.IsSafeExist())
            {
                return;
            }

            ped.SetNotChaosPed(false);
        }

        [Serializable]
        private class CitizenNitroConfig : InfernoConfig
        {
            private int _probability = 7;
            private int _intervalSeconds = 3;
            private int _range = 200;

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
        public override string DisplayName => EntitiesLocalize.CitizenNitroTitle;

        public override string Description => EntitiesLocalize.CitizenNitroDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Entities;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            subMenu.AddSlider(
                $"Range: {config.Range}[m]",
                EntitiesLocalize.CitizenNitroProbability,
                config.Range,
                1000,
                x =>
                {
                    x.Value = config.Range;
                    x.Multiplier = 10;
                }, item =>
                {
                    config.Range = item.Value;
                    item.Title = $"Range: {config.Range}[m]";
                });

            subMenu.AddSlider(
                $"Interval: {config.IntervalSeconds}[s]",
                EntitiesLocalize.CitizenNitroInterval,
                config.IntervalSeconds,
                60,
                x =>
                {
                    x.Value = config.IntervalSeconds;
                    x.Multiplier = 1;
                }, item =>
                {
                    config.IntervalSeconds = item.Value;
                    item.Title = $"Interval: {config.IntervalSeconds}[s]";
                });

            subMenu.AddSlider(
                $"Probability: {config.IntervalSeconds}[%]",
                EntitiesLocalize.CitizenNitroProbability,
                config.IntervalSeconds,
                100,
                x =>
                {
                    x.Value = config.Probability;
                    x.Multiplier = 1;
                }, item =>
                {
                    config.Probability = item.Value;
                    item.Title = $"Probability: {config.Probability}[%]";
                });

            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                config = LoadDefaultConfig<CitizenNitroConfig>();
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(config);
                DrawText($"Saved to {ConfigFileName}");
            });
        }

        #endregion
    }
}