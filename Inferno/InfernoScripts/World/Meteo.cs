using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;

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
               .Where(_ => IsActive && Random.Next(0,100) <= 30)
                .Subscribe(_ => ShootMeteo());
        }

        private void ShootMeteo()
        {
            try
            {
                var player = PlayerPed;
                if(!player.IsSafeExist()) return;

                var playerPosition = player.Position;
                var range = 30;
                var addPosition = new Vector3(0, 0, 0).AroundRandom2D(range);

                if (IsPlayerMoveSlowly && addPosition.Length() < 10.0f)
                {
                    addPosition.Normalize();
                    addPosition *= Random.Next(10, 20);
                }

                var targetPosition = playerPosition + addPosition;
                var direction = new Vector3(1,0,2);
                direction.Normalize();
                var createPosition = targetPosition + direction*100;

                //たまに花火
                var weapon = Random.Next(0, 100) < 3 ? (int)Weapon.FIREWORK : (int)Weapon.RPG;

                var ped = NativeFunctions.CreateRandomPed(createPosition);
                if(!ped.IsSafeExist()) return;
                ped.MarkAsNoLongerNeeded();
                if (!ped.IsSafeExist()) return;
                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.SetNotChaosPed(true);
                ped.GiveWeapon(weapon, 1); //指定武器所持
                ped.EquipWeapon(weapon); //武器装備
                ped.IsVisible = false;
                ped.FreezePosition = true;
                ped.SetPedFiringPattern((int)FiringPattern.SingleShot);

                //ライト描画
                StartCoroutine(CreateMeteoLight(targetPosition, 2.0f));

                //Aさん削除
                StartCoroutine(MeteoShoot(ped,targetPosition, 8.0f));

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
        private IEnumerable<object>  CreateMeteoLight(Vector3 position, float durationSecond)
        {
            meteoLightPositionList.Add(position);
            yield return WaitForSeconds(durationSecond);
            meteoLightPositionList.Remove(position);
        }

        /// <summary>
        /// 指定時間後にAさんを削除する
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="durationSecond"></param>
        /// <returns></returns>
        private IEnumerable<object> MeteoShoot(Ped ped,Vector3 targetPosition, float durationSecond)
        {
            ped.TaskShootAtCoord(targetPosition, 1000);
            yield return WaitForSeconds(durationSecond);
            if(!ped.IsSafeExist()) yield break;
            ped.Delete();
        }
    }
}
