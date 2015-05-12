using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno
{
    internal class Meteo : InfernoScript
    {

        private int _rpgHash;
        private bool _isActive = false;
        private List<Vector3> meteoLightPositionList = new List<Vector3>(); 

        //OnTickAsObservableはライト描画用に使う
        protected override int TickInterval
        {
            get { return 0; }
        }

        protected override void Setup()
        {
            _rpgHash = this.GetGTAObjectHashKey("WEAPON_RPG");

            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    DrawText("Meteo:" + _isActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => _isActive = true);

            //落下地点マーカ描画
            OnTickAsObservable
                .Where(_ => meteoLightPositionList.Count > 0)
                .Subscribe(_ =>
                {
                    foreach (var point in meteoLightPositionList)
                    {
                        NativeFunctions.CreateLight(point, 255, 0, 0, 0.8f, 5);
                    }
                });
                

            //１秒間隔で落下
            CreateTickAsObservable(1500)
                .Where(_ => _isActive)
                .Subscribe(_ => ShootMeteo());
        }

        private void ShootMeteo()
        {
            try
            {
                var player = this.GetPlayer();
                var playerPosition = player.Position;

                var addPosition = new Vector3(Random.Next(-40, 40), Random.Next(-40, 40), 0);

                if (player.Velocity.Length() < 1.0f && addPosition.Length() < 10.0f)
                {
                    addPosition.Normalize();
                    addPosition *= Random.Next(10, 40);
                }

                var targetPosition = playerPosition + addPosition; 

                var createPosition = targetPosition + new Vector3(50, 0, 100);
               

                var ped = NativeFunctions.CreateRandomPed(createPosition);
                ped.MarkAsNoLongerNeeded();
                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.GiveWeapon(_rpgHash, 1000); //指定武器所持
                ped.EquipWeapon(_rpgHash); //武器装備
                ped.IsVisible = false;
                ped.FreezePosition = true;
                ped.SetVisible(false);
                ped.TaskShootAtCoord(targetPosition, 1000);

                //ライト描画
                StartCoroutine(CreateMeteoLight(targetPosition, 1.0f));

                //Aさん削除
                StartCoroutine(DeleteMeteoShooter(ped, 5.0f));

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
        private IEnumerable<Object>  CreateMeteoLight(Vector3 position, float durationSecond)
        {
            meteoLightPositionList.Add(position);

            foreach (var s in WaitForSecond(durationSecond))
            {
                yield return s;
            }

            meteoLightPositionList.Remove(position);
        }

        /// <summary>
        /// 指定時間後にAさんを削除する
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="durationSecond"></param>
        /// <returns></returns>
        private IEnumerable<Object> DeleteMeteoShooter(Ped ped, float durationSecond)
        {
            foreach (var s in WaitForSecond(durationSecond))
            {
                yield return s;
            }

            ped.Delete();
        }
    }
}
