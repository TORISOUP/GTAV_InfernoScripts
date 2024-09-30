using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;

namespace Inferno
{
    internal sealed class Meteor : InfernoScript
    {
        private bool IsPlayerMoveSlowly => PlayerPed.Velocity.Length() < 5.0f;

        private bool _isDebug = false;

        protected override void Setup()
        {
            _config = LoadConfig<MeteorConfig>();
            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Meteor:" + IsActive);
                });

            IsActiveRP
                .Where(x => x)
                .Subscribe(_ =>
                {
                    var m = new Model(WeaponHash.RPG);
                    m.Request();
                    MeteorLoopAsync(ActivationCancellationToken).Forget();
                    DebugLoopAsync(ActivationCancellationToken).Forget();
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);
        }

        private async ValueTask MeteorLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Random.Next(0, 99) < Probability)
                {
                    ShootMeteorAsync(ct).Forget();
                }

                await DelayAsync(TimeSpan.FromMilliseconds(DurationMillSeconds), ct);
            }
        }

        private async ValueTask DebugLoopAsync(CancellationToken ct)
        {
            var color = Color.FromArgb(100, 0, 255, 0);
            while (!ct.IsCancellationRequested)
            {
                if (!_isDebug)
                {
                    await DelaySecondsAsync(1, ct);
                    continue;
                }

                var player = PlayerPed;
                if (!player.IsSafeExist())
                {
                    await DelaySecondsAsync(1, ct);
                    continue;
                }

                var centerPosition = GetOffsetCenterPosition();
                var range = Radius;

                NativeFunctions.DrawSphere(centerPosition, range, color);
                await YieldAsync(ct);
            }
        }

        private Vector3 GetOffsetCenterPosition()
        {
            var playerVelocity = PlayerPed.Velocity;
            return PlayerPed.Position + playerVelocity;
        }

        private async ValueTask ShootMeteorAsync(CancellationToken ct)
        {
            try
            {
                var (result, targetPosition) = CreateTargetPosition();

                if (!result)
                {
                    return;
                }

                // 斜めに降らせるためにちょっとずれた位置の上空から落とす
                var direction = new Vector3(1, 0, 2);
                direction.Normalize();
                var createPosition = targetPosition + direction * 100;

                //たまに花火
                var weapon = Random.Next(0, 100) < 3
                    ? Weapon.Firework
                    : Weapon.RPG;

                //　マーカー描画
                CreateMeteorMarkerAsync(targetPosition, 3.0f, ActivationCancellationToken).Forget();

                // 先に警告表示を出してちょっと待ってから落下
                await DelaySecondsAsync(1, ct);

                NativeFunctions.ShootSingleBulletBetweenCoords(
                    createPosition,
                    targetPosition, 200, weapon, null, -1.0f);
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }
        }

        private (bool, Vector3) CreateTargetPosition()
        {
            for (var i = 0; i < 3; i++)
            {
                var player = PlayerPed;
                if (!player.IsSafeExist())
                {
                    return (false, default);
                }

                // プレイヤーの移動速度に応じて補正をする
                var centerPosition = GetOffsetCenterPosition();
                var range = Radius;

                // プレイヤーの移動速度が遅い場合は近くには降らせない

                var addPosition =
                    IsPlayerMoveSlowly
                        ? new Vector3(0, 0, 0).Around(Random.RandomFloat(5, range))
                        : new Vector3(0, 0, 0).Around(Random.RandomFloat(0, range));

                var targetPosition = centerPosition + addPosition;

                var isNearMissionEntity =
                    CachedMissionEntities.Value.Any(x => x.Position.DistanceTo2D(targetPosition) < 30.0f);

                // ミッションキャラクターの近くが選ばれた場合は再抽選
                if (isNearMissionEntity)
                {
                    continue;
                }

                return (true, targetPosition);
            }

            return (false, default);
        }

        /// <summary>
        /// ライトを生成して指定秒数後に無効化する
        /// </summary>
        /// <param name="position"></param>
        /// <param name="durationSecond"></param>
        /// <returns></returns>
        private async ValueTask CreateMeteorMarkerAsync(Vector3 position, float durationSecond, CancellationToken ct)
        {
            var elapsed = 0.0f;
            var gh = World.GetGroundHeight(position);
            position = new Vector3(position.X, position.Y, gh);
            while (!ct.IsCancellationRequested && elapsed <= durationSecond)
            {
                if (!PlayerPed.IsSafeExist())
                {
                    return;
                }

                elapsed += DeltaTime;

                var markerType = (int)((durationSecond - elapsed) / durationSecond * 10) + 10;

                var playerSpeed = PlayerPed.Velocity.Length();

                // プレイヤーの移動速度に応じてmarkerThresholdを変える
                var markerThreshold = 30 * (playerSpeed / 30f).Clamp(0f, 1f) + 20;

                var lenght = (PlayerPed.Position - position).Length();
                // マーカー描画範囲内
                if (lenght <= markerThreshold)
                {
                    var alpha = 150 * ((markerThreshold - lenght) / markerThreshold).Clamp(0, 1f);

                    Function.Call(Hash.DRAW_MARKER,
                        markerType, // markerType
                        position.X, position.Y, position.Z + 0.5f,
                        0.0, 0.0, 0.0, // direction
                        0.0, 0.0, 0.0, // rot 
                        0.5f, 0.5f, 0.5f, // scale
                        255, 255, 255, (int)alpha, // color 
                        false, true, 2, null, null, false);
                }

                NativeFunctions.CreateLight(position + Vector3.WorldUp, 255, 50, 50, 3f, 500f);


                await YieldAsync(ct);
            }
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => IsLangJpn ? "メテオ" : "Meteor";

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.World;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            // 落下範囲


            subMenu.AddSlider(
                $"Range {_config.Radius}[m]",
                IsLangJpn? "メテオの落下範囲" : "Meteor fall range",
                _config.Radius,
                100,
                x => x.Multiplier = 5, item =>
                {
                    item.Title = $"Range {item.Value}[m]";
                    _config.Radius = item.Value;
                });


            // 落下間隔
            subMenu.AddSlider(
                $"Duration {_config.DurationMillSeconds}[ms]",
                IsLangJpn? "メテオの落下頻度" :"Meteor fall duration",
                _config.DurationMillSeconds,
                10000,
                x => x.Multiplier = 100, item =>
                {
                    item.Title = $"Duration {item.Value}[ms]";
                    _config.DurationMillSeconds = item.Value;
                });


            // 落下確率
            subMenu.AddSlider(
                $"Probability {_config.Probability}[%]",
                IsLangJpn? "メテオの落下確率" :"Meteor fall probability",
                _config.Probability,
                100,
                x => x.Multiplier = 5, item =>
                {
                    item.Title = $"Probability {item.Value}[%]";
                    _config.Probability = item.Value;
                });

            // Debug
            {
                var debugItem = new NativeCheckboxItem("Debug", _isDebug);
                debugItem.CheckboxChanged += (_, e) => _isDebug = debugItem.Checked;
                subMenu.Add(debugItem);
            }
        }

        #endregion

        #region config

        public class MeteorConfig : InfernoConfig
        {
            /// <summary>
            /// メテオ落下の最小範囲[m]
            /// </summary>
            [JsonProperty("Radius")]
            public int Radius
            {
                get => _radius;
                set => _radius = value.Clamp(0, 100);
            }

            /// <summary>
            /// メテオを落下させるのかの判定頻度[ms]
            /// </summary>
            [JsonProperty("DurationMillSeconds")]
            public int DurationMillSeconds
            {
                get => _durationMillSeconds;
                set => _durationMillSeconds = value.Clamp(100, 10000);
            }

            /// <summary>
            /// メテオを落下させる確率
            /// </summary>
            [JsonProperty("Probability")]
            public int Probability
            {
                get => _probability;
                set => _probability = value.Clamp(0, 100);
            }

            private int _radius = 30;
            private int _durationMillSeconds = 1000;
            private int _probability = 40;

            public override bool Validate()
            {
                return true;
            }

            public override string ToString()
            {
                return
                    $"{nameof(Radius)}: {Radius}, {nameof(DurationMillSeconds)}: {DurationMillSeconds}, {nameof(Probability)}: {Probability}";
            }
        }

        protected override string ConfigFileName { get; } = "Meteor.conf";
        private MeteorConfig _config;
        private float Radius => _config?.Radius ?? 30;
        private float DurationMillSeconds => _config?.DurationMillSeconds ?? 1000;
        private int Probability => _config?.Probability ?? 25;

        #endregion
    }
}