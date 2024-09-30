using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.InfernoScripts.World
{
    internal class SpeedMax : InfernoScript
    {
        private readonly HashSet<int> vehicleHashSet = new();
        private SpeedType currentSpeedType = SpeedType.Random;
        private bool excludeMissionVehicle;
        private float radius = 800;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("snax")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText($"SpeedMax:{IsActive}[Type:{currentSpeedType}][Exclude:{excludeMissionVehicle}]");
                });

            IsActiveRP
                .Subscribe(x => { vehicleHashSet.Clear(); });

            OnAllOnCommandObservable.Subscribe(_ =>
            {
                excludeMissionVehicle = true;
                IsActive = true;
            });

            //ミッション開始直後に一瞬動作を止めるフラグ
            var suspednFlag = false;

            OnThinnedTickAsObservable
                .Where(_ => IsActive && !suspednFlag)
                .Subscribe(_ =>
                {
                    foreach (var v in CachedVehicles
                                 .Where(x =>
                                     x.IsSafeExist()
                                     && x.IsInRangeOf(PlayerPed.Position, 100.0f)
                                     && !vehicleHashSet.Contains(x.Handle)
                                     && !(excludeMissionVehicle && x.IsPersistent)
                                 ))
                    {
                        vehicleHashSet.Add(v.Handle);

                        if (currentSpeedType == SpeedType.Random)
                        {
                            if (Random.Next(0, 100) < 90)
                            {
                                // ランダムの場合はたまにしか発動しない
                                return;
                            }
                        }

                        if (currentSpeedType == SpeedType.Max)
                        {
                            OriginalSpeedMaxAsync(v, ActivationCancellationToken).Forget();
                        }
                        else
                        {
                            VehicleSpeedMaxAsync(v, ActivationCancellationToken).Forget();
                        }
                    }
                });
            var nextType = currentSpeedType;
            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F6)
                .Do(_ =>
                {
                    nextType = GetNextSpeedType(nextType);
                    DrawText($"SpeedMax:[Type:{nextType}]*", 1.0f);
                })
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ => { ChangeSpeedType(nextType); });

            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F5)
                .Subscribe(_ =>
                {
                    excludeMissionVehicle = !excludeMissionVehicle;
                    vehicleHashSet.Clear();
                    DrawText($"SpeedMax:ExcludeMissionVehicles[{excludeMissionVehicle}]");
                });

            OnThinnedTickAsObservable
                .Where(_ => IsActive)
                .Select(_ => PlayerPed.IsAlive)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => vehicleHashSet.Clear());

            //ミッションが始まった時にしばらく動作を止める
            OnThinnedTickAsObservable
                .Where(_ => IsActive)
                .Select(_ => Game.IsMissionActive)
                .DistinctUntilChanged()
                .Where(x => x)
                .Do(_ => suspednFlag = true)
                .Delay(TimeSpan.FromSeconds(3))
                .Subscribe(_ => suspednFlag = false);
        }

        private void ChangeSpeedType(SpeedType nextType)
        {
            currentSpeedType = nextType;
            DrawText($"SpeedMax:[Type:{currentSpeedType}]", 2.0f);
            vehicleHashSet.Clear();
        }

        /// <summary>
        /// オリジナルに近い挙動
        /// </summary>
        private async ValueTask OriginalSpeedMaxAsync(Vehicle v, CancellationToken ct)
        {
            var maxSpeed = Random.Next(100, 300);
            try
            {
                while (IsActive && v.IsSafeExist() && !ct.IsCancellationRequested)
                {
                    if (v.IsInRangeOf(PlayerPed.Position, 800) && PlayerPed.CurrentVehicle != v)
                    {
                        v.SetForwardSpeed(maxSpeed);
                    }

                    await DelaySecondsAsync(1, ct);
                }
            }
            finally
            {
                vehicleHashSet.Remove(v.Handle);
            }
        }

        /// <summary>
        /// カスタム版
        /// </summary>
        private async ValueTask VehicleSpeedMaxAsync(Vehicle v, CancellationToken ct)
        {
            try
            {
                //たまに後ろに飛ぶ
                var dir = v.Handle % 10 == 0 ? -1 : 1;
                var maxSpeed = GetVehicleSpeed() * dir;


                while (v.IsSafeExist() && !ct.IsCancellationRequested)
                {
                    if (v.IsInRangeOf(PlayerPed.Position, 800) && PlayerPed.CurrentVehicle != v)
                    {
                        v.ApplyForce(maxSpeed * v.ForwardVector);
                        await DelayRandomSecondsAsync(0.2f, 1f, ct);
                        continue;
                    }

                    await DelaySecondsAsync(2f, ct);
                }
            }
            finally
            {
                vehicleHashSet.Remove(v.Handle);
            }
        }

        private SpeedType GetNextSpeedType(SpeedType current)
        {
            return (SpeedType)(((int)current + 1) % Enum.GetNames(typeof(SpeedType)).Length);
        }

        private float GetVehicleSpeed()
        {
            switch (currentSpeedType)
            {
                case SpeedType.Low:
                    return Random.Next(50, 100);
                case SpeedType.Middle:
                    return Random.Next(80, 150);
                case SpeedType.High:
                    return Random.Next(100, 500);
                case SpeedType.Random:
                    return Random.Next(100, 500);
                default:
                    return 0;
            }
        }

        private enum SpeedType
        {
            Max,
            High,
            Middle,
            Low,
            Random
        }


        #region UI

        public override bool UseUI => true;
        public override string DisplayName => IsLangJpn ? "スピードマックス" : "All vehicles super-accelerated";

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.World;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            // スピードタイプ
            {
                subMenu.AddEnumSlider($"Type: {currentSpeedType.ToString()}", "", SpeedType.Max,
                    x =>
                    {
                        x.Title = $"Type: {currentSpeedType.ToString()}";
                        x.Value = (int)currentSpeedType;
                    }, x =>
                    {
                        ChangeSpeedType((SpeedType)x.Value);
                        vehicleHashSet.Clear();
                        x.Title = $"Type: {currentSpeedType.ToString()}";
                    });
            }
        }

        #endregion
    }
}