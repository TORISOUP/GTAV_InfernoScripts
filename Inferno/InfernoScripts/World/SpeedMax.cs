using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;

namespace Inferno.InfernoScripts.World
{
    internal class SpeedMax : InfernoScript
    {
        private readonly HashSet<int> vehicleHashSet = new();
        private SpeedType currentSpeedType = SpeedType.Original;

        private bool excludeMissionVehicle;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("snax")
                .Subscribe(_ => IsActive = !IsActive);

            IsActiveAsObservable
                .Where(x => x)
                .Subscribe(x =>
                {
                    DrawText($"SpeedMax:{IsActive}[Type:{currentSpeedType}][Exclude:{excludeMissionVehicle}]");
                    vehicleHashSet.Clear();
                });

            IsActiveAsObservable
                .Skip(1)
                .Where(x => !x)
                .Subscribe(x =>
                {
                    DrawText($"SpeedMax:{IsActive}");
                    vehicleHashSet.Clear();
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
                        if (currentSpeedType == SpeedType.Original)
                            StartCoroutine(OriginalSpeedMaxCoroutine(v));
                        else
                            StartCoroutine(VehicleSpeedMaxCorutine(v));
                    }
                });
            var nextType = currentSpeedType;
            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F6)
                .Do(_ =>
                {
                    nextType = GetNextSpeedType(nextType);
                    DrawText($"SpeedMax:[Type:{nextType}]", 1.0f);
                })
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    currentSpeedType = nextType;
                    DrawText($"SpeedMax:[Type:{currentSpeedType}][OK]", 2.0f);
                    StopAllCoroutine();
                    vehicleHashSet.Clear();
                });

            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F5)
                .Subscribe(_ =>
                {
                    excludeMissionVehicle = !excludeMissionVehicle;
                    vehicleHashSet.Clear();
                    StopAllCoroutine();
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

        /// <summary>
        /// オリジナルに近い挙動
        /// </summary>
        private IEnumerable<object> OriginalSpeedMaxCoroutine(Vehicle v)
        {
            var maxSpeed = Random.Next(100, 300);
            while (IsActive && v.IsSafeExist())
            {
                if (!v.IsInRangeOf(PlayerPed.Position, 1000)) yield break;
                if (PlayerVehicle.Value == v) yield break;
                v.Speed = maxSpeed;
                yield return null;
            }
        }

        /// <summary>
        /// カスタム版
        /// </summary>
        private IEnumerable<object> VehicleSpeedMaxCorutine(Vehicle v)
        {
            //たまに後ろに飛ぶ
            var dir = v.Handle % 10 == 0 ? -1 : 1;
            var maxSpeed = GetVehicleSpeed() * dir;
            if (Math.Abs(maxSpeed) > 20) v.Speed = 100 * dir;
            while (IsActive && v.IsSafeExist())
            {
                if (!v.IsInRangeOf(PlayerPed.Position, 1000)) yield break;
                if (PlayerVehicle.Value == v) yield break;
                v.ApplyForce(maxSpeed * v.ForwardVector);
                yield return null;
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
                    return Random.Next(5, 10);
                case SpeedType.Middle:
                    return Random.Next(10, 15);
                case SpeedType.High:
                    return Random.Next(20, 30);
                case SpeedType.Random:
                    return Random.Next(5, 30);
                default:
                    return 0;
            }
        }

        private enum SpeedType
        {
            Original,
            Low,
            Middle,
            High,
            Random
        }
    }
}