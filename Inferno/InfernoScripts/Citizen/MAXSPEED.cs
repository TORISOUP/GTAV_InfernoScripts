using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno
{
    /// <summary>
    /// 市民の車両強盗
    /// </summary>
    class MaxSpeed : InfernoScript
    {

        private bool IsActive = false;
        private float PlayerAroundDistance = 200.0f;

        /// <summary>
        /// 3秒間隔
        /// </summary>
        protected override int TickInterval
        {
            get { return 3000; }
        }

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("runaway")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    LogWrite(IsActive.ToString());

                });

            OnTickAsObservable
                .Where(_ => IsActive)
                .Subscribe(_ => RunAway());
        }

        private void RunAway()
        {
            var player = this.GetPlayer();
            var playerVehicle = this.GetPlayerVehicle();

            //プレイヤ周辺の車
            var targetVehicles = CachedVehicles.Where(x => x.IsSafeExist()
                                                   && !x.IsSameEntity(this.GetPlayerVehicle())
                                                   && !x.IsPersistent//ミッション関連のものかどうか
                                                   && (x.Position - player.Position).Length() <= PlayerAroundDistance);

            foreach (var targetVehicle in targetVehicles)
            {
                try
                {
                    var driver =  Function.Call<Ped>(Hash.GET_PED_IN_VEHICLE_SEAT,targetVehicle, -1);//2個目の引数"-1"はどの席のPed取得するかというパラメータ
                    if (driver.IsSafeExist())
                    {
                        Function.Call(Hash.SET_DRIVE_TASK_MAX_CRUISE_SPEED,driver, 300.0f);
                        Function.Call(Hash.SET_DRIVE_TASK_CRUISE_SPEED, driver, 300.0f);
                    }
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
            }
        }
    }
}
