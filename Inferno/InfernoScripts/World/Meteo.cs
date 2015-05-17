using System;
using System.Collections;
using System.Collections.Generic;
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

        private int _rpgHash;
        private bool _isActive = false;
        private List<Vector3> meteoLightPositionList = new List<Vector3>();
        /// <summary>
        /// 夜間はマーカの色を薄くする
        /// </summary>
        private bool IsNightMode
        {
            get
            {
                var hour = NativeFunctions.GetClockHours();
                return hour >= 17 && hour <= 6;
            }
        }

        private bool IsPlayerMoveSlowly => this.GetPlayer().Velocity.Length() < 5.0f;

        //OnTickAsObservableはライト描画用に使う
        protected override int TickInterval => 1500;

        protected override void Setup()
        {
            _rpgHash = (int) Weapon.RPG;

            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    DrawText("Meteo:" + _isActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => _isActive = true);

            //落下地点マーカ描画
            OnDrawingTickAsObservable
                .Where(_ => meteoLightPositionList.Count > 0)
                .Subscribe(_ =>
                {
                    var insensity = IsNightMode ? 0.3f : 20.0f;
                    foreach (var point in meteoLightPositionList)
                    {

                        NativeFunctions.CreateLight(point, 255, 0, 0, 1.0f, insensity);
                    }
                });
                

            OnTickAsObservable
                .Where(_ => _isActive)
                .Subscribe(_ => ShootMeteo());
        }

        private void ShootMeteo()
        {
            try
            {
                var player = this.GetPlayer();
                var playerPosition = player.Position;
                var range = 30;
                var addPosition = new Vector3(0, 0, 0).AroundRandom2D(range);

                if (IsPlayerMoveSlowly && addPosition.Length() < 10.0f)
                {
                    addPosition.Normalize();
                    addPosition *= Random.Next(10, 30);
                }

                var targetPosition = playerPosition + addPosition; 

                var createPosition = targetPosition + new Vector3(50, 0, 100);
               

                var ped = NativeFunctions.CreateRandomPed(createPosition);
                ped.MarkAsNoLongerNeeded();
                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.SetNotChaosPed(true);
                ped.GiveWeapon(_rpgHash, 1000); //指定武器所持
                ped.EquipWeapon(_rpgHash); //武器装備
                ped.IsVisible = false;
                ped.FreezePosition = true;
    
                //ライト描画
                StartCoroutine(CreateMeteoLight(targetPosition, 3.0f));

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
