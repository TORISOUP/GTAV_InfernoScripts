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
        private float PlayerAroundDistance = 100.0f;

        /// <summary>
        /// 車両強盗する確率
        /// </summary>
        private readonly int probability = 20;

        /// <summary>
        /// 5秒間隔
        /// </summary>
        protected override int TickInterval => 5000;

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
            if(!playerPed.IsSafeExist())return;

            var playerVehicle = this.GetPlayerVehicle();

            //プレイヤの周辺の市民
            var targetPeds = CachedPeds.Where(x => x.IsSafeExist()
                                                   && !x.IsSameEntity(playerPed)
                                                   && !x.IsRequiredForMission()
                                                   && x.IsInRangeOf(playerPed.Position,PlayerAroundDistance));

            foreach (var targetPed in targetPeds)
            {
                try
                {
                    //確率で強盗する
                    if (Random.Next(0, 100) > probability)
                    {
                        continue;
                    }

                    //市民周辺の車が対象
                    var targetVehicle = World.GetNearbyVehicles(targetPed, 40).FirstOrDefault();

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
            if (!ped.IsSafeExist()) yield break;
            //カオス化しない
            ped.SetNotChaosPed(true);
            ped.TaskSetBlockingOfNonTemporaryEvents(false);
            ped.Task.ClearAll();
            ped.SetPedKeepTask(true);
            ped.Task.EnterVehicle(targetVehicle, VehicleSeat.Any);

            foreach (var t in Enumerable.Range(0,20))
            {
                //20秒間車に乗れたか監視する
                if(!ped.IsSafeExist()) yield break;
                if(ped.IsInVehicle()) break;
                yield return WaitForSeconds(1);
            }
            ped.TaskSetBlockingOfNonTemporaryEvents(true);

            //カオス化許可
            ped.SetNotChaosPed(false);
        } 

    }
}