using GTA;
using Inferno.ChaosMode;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno
{
    /// <summary>
    /// 市民の車両強盗
    /// </summary>
    internal class CitizenRobberVehicle : InfernoScript
    {
        /// <summary>
        /// プレイヤの周囲何ｍの市民が対象か
        /// </summary>
        private float PlayerAroundDistance = 100.0f;

        /// <summary>
        /// 車両強盗する確率
        /// </summary>
        private readonly int probability = 20;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("robber")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenRobberVehicle:" + IsActive, 3.0f);
                });

            CreateTickAsObservable(TimeSpan.FromSeconds(1))
                .Where(_ => IsActive)
                .Subscribe(_ => RobberVehicle());

            //デフォルトではOFFにする
            //   OnAllOnCommandObservable.Subscribe(_ => IsActive = true);
        }

        private void RobberVehicle()
        {
            if (!PlayerPed.IsSafeExist()) return;

            var playerVehicle = this.GetPlayerVehicle();

            //プレイヤの周辺の市民
            var targetPeds = CachedPeds.Where(x => x.IsSafeExist()
                                                   && x.IsAlive
                                                   && x.IsInRangeOf(PlayerPed.Position, PlayerAroundDistance)
                                                   && x != PlayerPed
                                                   && !x.IsRequiredForMission()
                                                   && !x.IsNotChaosPed());

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
                    var targetVehicle =
                        CachedVehicles
                            .FirstOrDefault(x => x.IsSafeExist() && x.IsInRangeOf(targetPed.Position, 40.0f) && x != targetPed.CurrentVehicle);

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

        private IEnumerable<Object> RobberVehicleCoroutine(Ped ped, Vehicle targetVehicle)
        {
            yield return RandomWait();
            if (!ped.IsSafeExist()) yield break;
            //カオス化しない
            ped.SetNotChaosPed(true);

            ped.TaskSetBlockingOfNonTemporaryEvents(true);
            ped.SetPedKeepTask(true);
            ped.AlwaysKeepTask = true;

            if (ped.IsInVehicle())
            {
                ped.Task.ClearAll();
                ped.Task.LeaveVehicle(ped.CurrentVehicle, false);
                yield return WaitForSeconds(1);
            }
            else
            {
                ped.Task.ClearAllImmediately();
            }
            ped.Task.ClearAllImmediately();
            ped.Task.EnterVehicle(targetVehicle, VehicleSeat.Any);

            foreach (var t in Enumerable.Range(0, 5))
            {
                //20秒間車に乗れたか監視する
                if (!ped.IsSafeExist()) yield break;
                if (ped.IsInVehicle()) break;
                ped.Task.ClearAllImmediately();
                yield return WaitForSeconds(5);
            }

            if (!ped.IsSafeExist()) yield break;

            //カオス化許可
            ped.SetNotChaosPed(false);
        }
    }
}