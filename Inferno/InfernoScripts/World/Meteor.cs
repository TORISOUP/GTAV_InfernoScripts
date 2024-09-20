using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Utilities;
using LemonUI.Menus;

namespace Inferno
{
    internal sealed class Meteor : InfernoScript
    {
        private bool IsPlayerMoveSlowly => PlayerPed.Velocity.Length() < 5.0f;

        protected override void Setup()
        {
            _config = LoadConfig<MeteorConfig>();
            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Meteor:" + IsActive);
                });

            IsActivePR.Where(x => x)
                .Subscribe(_ => MeteorLoopAsync(ActivationCancellationToken).Forget());

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);
        }

        private async ValueTask MeteorLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Random.Next(0, 100) <= Probability)
                {
                    ShootMeteorAsync(ct).Forget();
                }

                await DelayAsync(TimeSpan.FromMilliseconds(DurationMillSeconds), ct);
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
                    : Weapon.VEHICLE_ROCKET;

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
                        ? new Vector3(0, 0, 0).Around(RandomFloat(5, range))
                        : new Vector3(0, 0, 0).Around(RandomFloat(0, range));

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
                var markerThreshold = 30 * Clamp(playerSpeed / 30f, 0, 1) + 20;

                var lenght = (PlayerPed.Position - position).Length();
                // マーカー描画範囲内
                if (lenght <= markerThreshold)
                {
                    var alpha = 150 * Clamp((markerThreshold - lenght) / markerThreshold, 0, 1f);

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

        private float RandomFloat(float min, float max)
        {
            return (float)Random.NextDouble() * (max - min) + min;
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        #region config

        [Serializable]
        public class MeteorConfig : InfernoConfig
        {
            /// <summary>
            /// メテオ落下の最小範囲[m]
            /// </summary>
            public int Radius = 30;

            /// <summary>
            /// メテオを落下させるのかの判定頻度[ms]
            /// </summary>
            public int DurationMillSeconds = 1000;

            /// <summary>
            /// メテオを落下させる確率
            /// </summary>
            public int Probability = 40;

            public override bool Validate()
            {
                if (Radius <= 0)
                {
                    return false;
                }

                if (DurationMillSeconds <= 0)
                {
                    return false;
                }

                if (Probability <= 0 || Probability > 100)
                {
                    return false;
                }

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

        public bool CanChangeActive => true;
        public bool NeedSubMenu => true;

        public void OnUiMenuConstruct(NativeMenu menu)
        {
        }
    }
}