﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno
{
    internal sealed class Meteor : InfernoScript
    {
        private readonly List<Vector3> _meteoLightPositionList = new();

        private bool IsPlayerMoveSlowly => PlayerPed.Velocity.Length() < 5.0f;

        protected override void Setup()
        {
            config = LoadConfig<MeteoConfig>();

            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Meteor:" + IsActive);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //落下地点マーカ描画
            OnDrawingTickAsObservable
                .Where(_ => _meteoLightPositionList.Count > 0)
                .Subscribe(_ =>
                {
                    var insensity = 10;
                    foreach (var point in _meteoLightPositionList.ToArray())
                    {
                        NativeFunctions.CreateLight(point, 255, 0, 0, 1.0f, insensity);
                    }
                });

            CreateTickAsObservable(TimeSpan.FromMilliseconds(DurationMillSeconds))
                .Where(_ => IsActive && Random.Next(0, 100) <= Probability)
                .Subscribe(_ => { ShootMeteo(); });
        }

        private void ShootMeteo()
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
                    ?  Weapon.Firework
                    :  Weapon.VEHICLE_ROCKET;

                //ライト描画
                CreateMeteoLightAsync(targetPosition, 2.0f, ActivationCancellationToken).Forget();

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

                var playerPosition = player.Position;
                var range = Radius;

                // ランダムな水平ベクトル
                var addPosition = new Vector3(0, 0, 0).AroundRandom2D(range);

                // プレイヤーの移動速度に応じて補正をする
                if (IsPlayerMoveSlowly && addPosition.Length() < 10.0f)
                {
                    // プレイヤーがゆっくり移動しているなら落下範囲をより狭くする
                    addPosition.Normalize();
                    addPosition *= Random.Next(10, 20);
                }

                var targetPosition = playerPosition + addPosition;

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
        private async ValueTask CreateMeteoLightAsync(Vector3 position, float durationSecond, CancellationToken ct)
        {
            _meteoLightPositionList.Add(position);
            await DelaySecondsAsync(durationSecond, ct);
            _meteoLightPositionList.Remove(position);
        }

        #region config

        private class MeteoConfig : InfernoConfig
        {
            /// <summary>
            /// プレイヤを中心としたメテオ落下範囲[m]
            /// </summary>
            public int Radius { get; } = 30;

            /// <summary>
            /// メテオを落下させるのかの判定頻度[ms]
            /// </summary>
            public int DurationMillSeconds { get; } = 1000;

            /// <summary>
            /// メテオを落下させる確率
            /// </summary>
            public int Probability { get; } = 25;

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
        }

        protected override string ConfigFileName { get; } = "Meteor.conf";
        private MeteoConfig config;
        private int Radius => config?.Radius ?? 30;
        private int DurationMillSeconds => config?.DurationMillSeconds ?? 1000;
        private int Probability => config?.Probability ?? 25;

        #endregion
    }
}