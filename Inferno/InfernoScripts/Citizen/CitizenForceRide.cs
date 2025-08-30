using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
    public sealed class CitizenForceRide : InfernoScript
    {
        private CompositeDisposable _activationDisposable;

        private HashSet<int> _processingPeds;

        private ForceRideConfig _config;

        protected override string ConfigFileName { get; } = "ForceRide.conf";

        protected override void Setup()
        {
            _config ??= LoadConfig<ForceRideConfig>();

            CreateInputKeywordAsObservable("ForceRide", "forceride")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("ForceRide:" + IsActive);
                });

            IsActiveRP.Subscribe(x =>
            {
                if (x)
                {
                    Activated();
                }
                else
                {
                    Deactivated();
                }
            });
        }

        private void Activated()
        {
            _processingPeds ??= new HashSet<int>();
            _activationDisposable?.Dispose();
            _activationDisposable = new CompositeDisposable();
            
            var hornPressed = OnTickAsObservable
                .Select(_ => Game.IsControlJustPressed(Control.VehicleHorn))
                .DistinctUntilChanged()
                .Where(s => s);

            hornPressed
                .Buffer(() => hornPressed.Throttle(TimeSpan.FromSeconds(_config.Period), InfernoScheduler))
                .Where(buf => buf.Count >= _config.Hit)
                .Subscribe(_ => RideAroundPedOnPlayerVehicle())
                .AddTo(_activationDisposable);
        }

        private void Deactivated()
        {
            _activationDisposable?.Dispose();
            _activationDisposable = null;
            _processingPeds?.Clear();
            _processingPeds = null;
        }

        private void RideAroundPedOnPlayerVehicle()
        {
            if (!PlayerPed.IsInVehicle()) return;
            if (!IsActive) return;
            
            DrawTextL("Force ride!");

            // 周辺市民の探索
            var aroundPeds = CachedPeds.Around(PlayerPed, 10f)
                .Where(x => !x.IsInVehicle() && !_processingPeds.Contains(x.Handle))
                .ToArray();

            // ミッション対象者がいるならその人を最優先
            var missionCharacter = aroundPeds.FirstOrDefault(x => x.IsRequiredForMission());
            if (missionCharacter.IsSafeExist())
            {
                ObservePedToVehicleAsync(missionCharacter, ActivationCancellationToken).Forget();
                return;
            }

            foreach (var ped in aroundPeds)
            {
                ObservePedToVehicleAsync(ped, ActivationCancellationToken).Forget();
            }
        }


        /// <summary>
        /// 対象市民をPlayerが乗っている車にのせる
        /// </summary>
        private async ValueTask ObservePedToVehicleAsync(Ped ped, CancellationToken ct)
        {
            if (!PlayerPed.IsInVehicle()) return;
            if (!_processingPeds.Add(ped.Handle)) return;

            try
            {
                ped.SetNotChaosPed(true);
                ped.AlwaysKeepTask = false;
                ped.BlockPermanentEvents = true;
                ped.Task.ClearAllImmediately();
                ped.Task.ClearSecondary();
                ped.Task.ClearLookAt();


                var playerVehicle = PlayerPed.CurrentVehicle;
                if (!playerVehicle.IsSafeExist()) return;

                // たまに運転席
                var targetSeat = Random.Next(0, 100) < 10 ? VehicleSeat.Driver : VehicleSeat.Any;

                var retryCount = 5;
                // 車に乗るまでリトライ
                while (ped.IsSafeExist() && !ped.IsGettingIntoVehicle && PlayerPed.IsInVehicle())
                {
                    // 中止
                    if (retryCount == 0) return;
                    ped.Task.EnterVehicle(playerVehicle, targetSeat, -1, 2.0f,
                        EnterVehicleFlags.JackAnyone | EnterVehicleFlags.ResumeIfInterupted);

                    await DelaySecondsAsync(1, ct);
                    retryCount--;
                }

                if (!ped.IsSafeExist()) return;


                // 車に乗るまで監視
                while (ped.IsSafeExist() && !ped.IsInVehicle() && ped.IsGettingIntoVehicle && PlayerPed.IsInVehicle() &&
                       !ct.IsCancellationRequested)
                {
                    if (!ped.IsAlive) return;
                    await DelaySecondsAsync(1, ct);
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    ped.BlockPermanentEvents = false;
                    ped.SetNotChaosPed(false);
                }

                _processingPeds?.Remove(ped.Handle);
            }
        }

        public override bool UseUI => true;
        public override string DisplayName => EntitiesLocalize.ForceRideTitle;

        public override string Description => EntitiesLocalize.ForceRideDescription;

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Entities;

        [Serializable]
        private class ForceRideConfig : InfernoConfig
        {
            private int _hit = 3;
            private float _period = 2.0f;

            public int Hit
            {
                get => _hit;
                set => _hit = value.Clamp(2, 10);
            }

            public float Period
            {
                get => _period;
                set => _period = value.Clamp(1f, 10f);
            }

            public override bool Validate()
            {
                if (_period < 1) return false;
                if (_period > 10) return false;
                if (_hit < 2) return false;
                if (_hit > 10) return false;
                return true;
            }
        }



        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            _config ??= LoadConfig<ForceRideConfig>();

            var button = subMenu.AddButton(EntitiesLocalize.ForceRideAction, "",
                _ =>
                {
                    InfernoSynchronizationContext.Post(_=> RideAroundPedOnPlayerVehicle(), null);
                });

            IsActiveRP.Subscribe(x => button.Enabled = x);

            subMenu.AddSlider(
                $"Input timeout: {_config.Period}[s]",
                EntitiesLocalize.ForceRideInputTimeout,
                (int)(10 * _config.Period),
                100,
                x =>
                {
                    x.Value = (int)(10 * _config.Period);
                    x.Multiplier = 1;
                }, item =>
                {
                    var next = item.Value / 10f;
                    if (Math.Abs(next - _config.Period) > 0.001f)
                    {
                        IsActive = false;
                    }

                    _config.Period = next;
                    item.Title = $"Input timeout: {_config.Period}[s]";
                });

            subMenu.AddSlider(
                $"Require hits: {_config.Hit}",
                EntitiesLocalize.ForceRideInputCount,
                _config.Hit,
                10,
                x =>
                {
                    x.Value = _config.Hit;
                    x.Multiplier = 1;
                }, item =>
                {
                    var next = item.Value;
                    if (next != _config.Hit)
                    {
                        IsActive = false;
                    }

                    _config.Hit = item.Value;
                    item.Title = $"Require hits: {_config.Hit}";
                });


            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                _config = LoadDefaultConfig<ForceRideConfig>();
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(_config);
                DrawText($"Saved to {ConfigFileName}");
            });
        }
    }
}