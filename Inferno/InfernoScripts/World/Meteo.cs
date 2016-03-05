using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using System;
using System.Collections.Generic;
using System.Linq;
using GTA.Native;
using UniRx;

namespace Inferno
{
    internal class Meteo : InfernoScript
    {
        private readonly List<Vector3> meteoLightPositionList = new List<Vector3>();

        private bool IsPlayerMoveSlowly => PlayerPed.Velocity.Length() < 5.0f;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Meteo:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //落下地点マーカ描画
            OnDrawingTickAsObservable
                .Where(_ => meteoLightPositionList.Count > 0)
                .Subscribe(_ =>
                {
                    var insensity = 10;
                    foreach (var point in meteoLightPositionList.ToArray())
                    {
                        NativeFunctions.CreateLight(point, 255, 0, 0, 1.0f, insensity);
                    }
                });

            CreateTickAsObservable(1000)
               .Where(_ => IsActive && Random.Next(0, 100) <= 25)
                .Subscribe(_ => ShootMeteo());
        }

        private void ShootMeteo()
        {
            try
            {
                var player = PlayerPed;
                if (!player.IsSafeExist()) return;

                var playerPosition = player.Position;
                var range = 30;
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
                var weapon = Random.Next(0, 100) < 3 ? (int)Weapon.FIREWORK : (int)Weapon.RPG;

                //ライト描画
                StartCoroutine(CreateMeteoLight(targetPosition, 2.0f));

                //そこら辺の市民のせいにする
                var ped = CachedPeds.Where(x => x.IsSafeExist()).DefaultIfEmpty(PlayerPed).FirstOrDefault();

                NativeFunctions.ShootSingleBulletBetweenCoords(
                      createPosition, targetPosition, 100, WeaponHash.RPG, ped, 200);

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

    }
}
