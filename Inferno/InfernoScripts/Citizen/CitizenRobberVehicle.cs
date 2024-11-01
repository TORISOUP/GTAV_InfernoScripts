using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.ChaosMode;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// 市民の車両強盗
    /// </summary>
    internal class CitizenRobberVehicle : InfernoScript
    {
        /// <summary>
        /// プレイヤの周囲何ｍの市民が対象か
        /// </summary>
        private float PlayerAroundDistance => _conf.Range;

        /// <summary>
        /// 車両強盗する確率
        /// </summary>
        private int Probability => _conf.Probability;

        private int IntervalSeconds => _conf.IntervalSeconds;

        private CitizenRobberVehicleConf _conf;

        protected override string ConfigFileName { get; } = "CitizenRobberVehicle.conf";

        private HashSet<Ped> _robbingPeds = new();

        protected override void Setup()
        {
            _conf = LoadConfig<CitizenRobberVehicleConf>();

            CreateInputKeywordAsObservable("CitizenRobberVehicle", "robber")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenRobberVehicle:" + IsActive);
                });

            IsActiveRP.Subscribe(x =>
            {
                _robbingPeds.Clear();
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
                RobberVehicle();
                await DelaySecondsAsync(IntervalSeconds, ct);
            }
        }

        private void RobberVehicle()
        {
            if (!PlayerPed.IsSafeExist())
            {
                return;
            }

            var playerVehicle = this.GetPlayerVehicle();

            //プレイヤの周辺の市民
            var targetPeds = CachedPeds.Where(x => x.IsSafeExist()
                                                   && x.IsAlive
                                                   && x.IsInRangeOf(PlayerPed.Position, PlayerAroundDistance)
                                                   && x != PlayerPed
                                                   && !x.IsRequiredForMission()
                                                   && !_robbingPeds.Contains(x));

            foreach (var targetPed in targetPeds)
            {
                try
                {
                    //確率で強盗する
                    if (Random.Next(0, 100) > Probability)
                    {
                        continue;
                    }


                    _robbingPeds.Add(targetPed);

                    RobberVehicleAsync(targetPed, ActivationCancellationToken).Forget();
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
            }
        }

        private async ValueTask RobberVehicleAsync(Ped ped, CancellationToken ct)
        {
            if (!ped.IsSafeExist())
            {
                return;
            }

            //カオス化しない
            ped.SetNotChaosPed(true);
            try
            {
                //市民周辺の車が対象
                var targetVehicle =
                    CachedVehicles
                        .Where(x => x.IsSafeExist() && x != ped.CurrentVehicle &&
                                    x.IsInRangeOf(ped.Position, 40.0f))
                        .OrderBy(x => x.Position.DistanceTo(ped.Position))
                        .FirstOrDefault();

                //30%の確率でプレイヤの車を盗むように変更
                if (PlayerPed.CurrentVehicle.IsSafeExist() && Random.Next(0, 100) < 30)
                {
                    targetVehicle = PlayerPed.CurrentVehicle;
                }
                

                if (ped.IsInVehicle())
                {
                    ped.Task.ClearAll();
                    ped.Task.LeaveVehicle(ped.CurrentVehicle, LeaveVehicleFlags.None);

                    var timeoutSeconds = 5f;

                    while (timeoutSeconds > 0)
                    {
                        timeoutSeconds -= DeltaTime;
                        if (!ped.IsInVehicle())
                        {
                            break;
                        }

                        await YieldAsync(ct);
                    }
                }
                else
                {
                    ped.Task.ClearAllImmediately();
                }


                if (!targetVehicle.IsSafeExist())
                {
                    return;
                }

                ped.Task.ClearAll();
                ped.Task.EnterVehicle(targetVehicle, VehicleSeat.Any, -1, 2, EnterVehicleFlags.JackAnyone);
                
                
                for (var i = 0; i < 20; i++)
                {
                    //20秒間車に乗れたか監視する
                    if (!ped.IsSafeExist() || !ped.IsAlive || !targetVehicle.IsSafeExist())
                    {
                        break;
                    }

                    if (!ped.IsInVehicle())
                    {
                        ped.Task.EnterVehicle(targetVehicle, VehicleSeat.Any, -1, 2, EnterVehicleFlags.JackAnyone);
                    }
                    else
                    {
                        break;
                    }

                    await DelaySecondsAsync(1, ct);
                }
            }
            finally
            {
                _robbingPeds.Remove(ped);
                if (ped.IsSafeExist())
                {
                    //カオス化許可
                    ped.SetNotChaosPed(false);
                }
            }
        }

        [Serializable]
        public class CitizenRobberVehicleConf : InfernoConfig
        {
            private int _probability = 10;
            private int _intervalSeconds = 5;
            private int _range = 50;

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
        public override string DisplayName => EntitiesLocalize.RobberTitle;

        public override string Description => EntitiesLocalize.RobberDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Entities;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            subMenu.AddSlider(
                $"Range: {_conf.Range}[m]",
                EntitiesLocalize.RobberRange,
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
                EntitiesLocalize.RobberInterval,
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
                EntitiesLocalize.RobberProbability,
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
                _conf = LoadDefaultConfig<CitizenRobberVehicleConf>();
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