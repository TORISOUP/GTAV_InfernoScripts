using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno
{
    internal class Meteo : InfernoScript
    {
        private readonly List<Vector3> meteoLightPositionList = new();

        private bool IsPlayerMoveSlowly => PlayerPed.Velocity.Length() < 5.0f;

        protected override void Setup()
        {
            config = LoadConfig<MeteoConfig>();

            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Meteo:" + IsActive);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //落下地点マーカ描画
            OnDrawingTickAsObservable
                .Where(_ => meteoLightPositionList.Count > 0)
                .Subscribe(_ =>
                {
                    var insensity = 10;
                    foreach (var point in meteoLightPositionList.ToArray())
                        NativeFunctions.CreateLight(point, 255, 0, 0, 1.0f, insensity);
                });

            CreateTickAsObservable(TimeSpan.FromMilliseconds(DurationMillSeconds))
                .Where(_ => IsActive && Random.Next(0, 100) <= Probability)
                .Subscribe(_ => ShootMeteo());
        }

        private void ShootMeteo()
        {
            try
            {
                var player = PlayerPed;
                if (!player.IsSafeExist()) return;

                var playerPosition = player.Position;
                var range = Radius;
                var addPosition = new Vector3(0, 0, 0).AroundRandom2D(range);

                if (IsPlayerMoveSlowly && addPosition.Length() < 10.0f)
                {
                    addPosition.Normalize();
                    addPosition *= Random.Next(10, 20);
                }

                var targetPosition = playerPosition + addPosition;
                var direction = new Vector3(1, 0, 2);
                direction.Normalize();
                var createPosition = targetPosition + direction * 100;

                //たまに花火
                var weapon = Random.Next(0, 100) < 3 ? WeaponHash.Firework : WeaponHash.RPG;

                //ライト描画
                StartCoroutine(CreateMeteoLight(targetPosition, 2.0f));

                //そこら辺の市民のせいにする
                var ped = CachedPeds.Where(x => x.IsSafeExist()).DefaultIfEmpty(PlayerPed).FirstOrDefault();

                NativeFunctions.ShootSingleBulletBetweenCoords(
                    createPosition, targetPosition, 100, weapon, ped, 200);
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }
        }

        /// <summary>
        /// ライトを生成して指定秒数後に無効化する
        /// </summary>
        /// <param name="position"></param>
        /// <param name="durationSecond"></param>
        /// <returns></returns>
        private IEnumerable<object> CreateMeteoLight(Vector3 position, float durationSecond)
        {
            meteoLightPositionList.Add(position);
            yield return WaitForSeconds(durationSecond);
            meteoLightPositionList.Remove(position);
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
                if (Radius <= 0) return false;
                if (DurationMillSeconds <= 0) return false;
                if (Probability <= 0 || Probability > 100) return false;
                return true;
            }
        }

        protected override string ConfigFileName { get; } = "Meteo.conf";
        private MeteoConfig config;
        private int Radius => config?.Radius ?? 30;
        private int DurationMillSeconds => config?.DurationMillSeconds ?? 1000;
        private int Probability => config?.Probability ?? 25;

        #endregion
    }
}