using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Policy;
using GTA;
using Inferno.ChaosMode;

namespace Inferno
{
    /// <summary>
    /// 市民の車両強盗
    /// </summary>
    internal class CitizenRobberVehicle : InfernoScript
    {

        private bool _isActive = false;

        /// <summary>
        /// プレイヤの周囲何ｍの市民が対象か
        /// </summary>
        private float PlayerAroundDistance = 300.0f;

        /// <summary>
        /// 車両強盗する確率
        /// </summary>
        private readonly int probability = 10;

        /// <summary>
        /// 10秒間隔
        /// </summary>
        protected override int TickInterval
        {
            get { return 10000; }
        }

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("robber")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    DrawText("CitizenRobberVehicle:" + _isActive, 3.0f);
                });

            OnTickAsObservable
                .Where(_ => _isActive)
                .Subscribe(_ => RobberVehicle());

            OnAllOnCommandObservable.Subscribe(_ => _isActive = true);
        }

        private void RobberVehicle()
        {
            var player = this.GetPlayer();
            var playerVehicle = this.GetPlayerVehicle();

            //プレイヤの周辺の市民
            var targetPeds = CachedPeds.Where(x => x.IsSafeExist()
                                                   && !x.IsSameEntity(this.GetPlayer())
                                                   && !x.IsPersistent
                                                   && (x.Position - player.Position).Length() <= PlayerAroundDistance);

            foreach (var targetPed in targetPeds)
            {
                try
                {
                    //10%の確率で強盗する
                    if (Random.Next(0, 100) > probability)
                    {
                        continue;
                    }

                    //市民から10m以内の車が対象
                    var targetVehicle =
                        CachedVehicles.FirstOrDefault(
                            x => x.IsSafeExist()
                                 && (x.Position - targetPed.Position).Length() < 20.0f);

                    //30%の確率でプレイヤの車を盗むように変更
                    if (playerVehicle.IsSafeExist() && Random.Next(0, 100) < 30)
                    {
                        targetVehicle = playerVehicle;
                    }


                    if (!targetVehicle.IsSafeExist()) continue;
                    StartCoroutine(RobberVehicleCoroutine(targetPed, targetVehicle));
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
            }
        }

        IEnumerable<Object> RobberVehicleCoroutine(Ped ped, Vehicle targetVehicle)
        {
            //カオス化しない
            ped.SetNotChaosPed(true);
            ped.Task.ClearAll();
            ped.TaskEnterVehicle(targetVehicle, -1, GTA.VehicleSeat.Any);

            foreach (var t in WaitForSecond(20))
            {
                //20秒間車に乗れたか監視する
                if(ped.IsInVehicle()) break;
                yield return null;
            }

            //カオス化許可
            ped.SetNotChaosPed(false);
        } 

    }
}