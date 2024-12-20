﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.InfernoScripts.World
{
    internal class SpeedMax : InfernoScript
    {
        private readonly HashSet<int> _vehicleHashSet = new();
        private SpeedType _currentSpeedType = SpeedType.Random;
        private bool _excludeMissionVehicle = true;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("SpeedMax", "snax")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText($"SpeedMax:{IsActive}[Type:{_currentSpeedType}][Exclude:{_excludeMissionVehicle}]");
                });

            IsActiveRP
                .Subscribe(x =>
                {
                    _vehicleHashSet.Clear();
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
                                     && !_vehicleHashSet.Contains(x.Handle)
                                     && !(_excludeMissionVehicle && x.IsPersistent)
                                 ))
                    {
                        _vehicleHashSet.Add(v.Handle);

                        if (_currentSpeedType == SpeedType.Random)
                        {
                            if (Random.Next(0, 100) < 90)
                            {
                                // ランダムの場合はたまにしか発動しない
                                return;
                            }
                        }

                        if (_currentSpeedType == SpeedType.Max)
                        {
                            OriginalSpeedMaxAsync(v, ActivationCancellationToken).Forget();
                        }
                        else
                        {
                            VehicleSpeedMaxAsync(v, ActivationCancellationToken).Forget();
                        }
                    }
                });
            
            OnThinnedTickAsObservable
                .Where(_ => IsActive)
                .Select(_ => PlayerPed.IsAlive)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => _vehicleHashSet.Clear());

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
            _currentSpeedType = nextType;
            _vehicleHashSet.Clear();
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
                _vehicleHashSet.Remove(v.Handle);
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
                _vehicleHashSet.Remove(v.Handle);
            }
        }

        private SpeedType GetNextSpeedType(SpeedType current)
        {
            return (SpeedType)(((int)current + 1) % Enum.GetNames(typeof(SpeedType)).Length);
        }

        private float GetVehicleSpeed()
        {
            switch (_currentSpeedType)
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
        public override string DisplayName => SpeedMaxLocalize.Title;

        public override string Description => SpeedMaxLocalize.Description;

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.World;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            // スピードタイプ
            {
                subMenu.AddEnumSlider($"Type: {_currentSpeedType.ToString()}", SpeedMaxLocalize.SpeedTypeDescription,
                    SpeedType.Max,
                    x =>
                    {
                        x.Title = $"Type: {_currentSpeedType.ToString()}";
                        x.Value = (int)_currentSpeedType;
                    }, x =>
                    {
                        ChangeSpeedType((SpeedType)x.Value);
                        _vehicleHashSet.Clear();
                        x.Title = $"Type: {_currentSpeedType.ToString()}";
                    });
            }

            // ミッション車両の除外
            {
                subMenu.AddCheckbox(SpeedMaxLocalize.ExcludeMissionVehicles, "",
                    item => { item.Checked = _excludeMissionVehicle; }, x =>
                    {
                        _excludeMissionVehicle = x;
                        _vehicleHashSet.Clear();
                    });
            }
        }

        #endregion
    }
}