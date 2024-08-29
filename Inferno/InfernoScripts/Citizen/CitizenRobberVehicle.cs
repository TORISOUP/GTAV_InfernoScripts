using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.ChaosMode;
using Inferno.Utilities;

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
        private readonly float PlayerAroundDistance = 100.0f;

        /// <summary>
        /// 車両強盗する確率
        /// </summary>
        private readonly int probability = 5;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("robber")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenRobberVehicle:" + IsActive);
                });

            CreateTickAsObservable(TimeSpan.FromSeconds(1))
                .Where(_ => IsActive)
                .Subscribe(_ => RobberVehicle());

            //デフォルトではOFFにする
            //   OnAllOnCommandObservable.Subscribe(_ => IsActive = true);
        }

        private void RobberVehicle()
        {
            if (!PlayerPed.IsSafeExist())
            {
                return;
            }

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
                            .FirstOrDefault(x =>
                                x.IsSafeExist() && x.IsInRangeOf(targetPed.Position, 40.0f) &&
                                x != targetPed.CurrentVehicle);

                    //30%の確率でプレイヤの車を盗むように変更
                    if (playerVehicle.IsSafeExist() && Random.Next(0, 100) < 30)
                    {
                        targetVehicle = playerVehicle;
                    }

                    if (!targetVehicle.IsSafeExist())
                    {
                        continue;
                    }

                    RobberVehicleAsync(targetPed, targetVehicle, ActivationCancellationToken).Forget();
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
            }
        }

        private async ValueTask RobberVehicleAsync(Ped ped, Vehicle targetVehicle, CancellationToken ct)
        {
            await DelayRandomSecondsAsync(1, 2, ct);
            if (!ped.IsSafeExist())
            {
                return;
            }

            //カオス化しない
            ped.SetNotChaosPed(true);
            try
            {
                ped.TaskSetBlockingOfNonTemporaryEvents(true);
                ped.SetPedKeepTask(true);
                ped.AlwaysKeepTask = true;

                if (ped.IsInVehicle())
                {
                    ped.Task.ClearAll();
                    ped.Task.LeaveVehicle(ped.CurrentVehicle, false);
                    await DelayAsync(TimeSpan.FromSeconds(3), ct);
                }
                else
                {
                    ped.Task.ClearAllImmediately();
                }

                ped.Task.ClearAllImmediately();
                ped.Task.EnterVehicle(targetVehicle);

                for (var i = 0; i < 20; i++)
                {
                    //20秒間車に乗れたか監視する
                    if (!ped.IsSafeExist())
                    {
                        return;
                    }

                    if (ped.IsInVehicle())
                    {
                        break;
                    }

                    await DelaySecondsAsync(1, ct);
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    //カオス化許可
                    ped.SetNotChaosPed(false);
                }
            }
        }
    }
}