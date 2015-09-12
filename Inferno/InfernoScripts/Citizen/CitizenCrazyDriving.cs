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
    /// 市民の運転スピード上限増加
    /// </summary>
    internal class CitizenCrazyDriving : InfernoScript
    {
        private readonly float PlayerAroundDistance = 300f;
        private List<Entity> affectPeds = new List<Entity>(); 


        protected override void Setup()
        {
            CreateInputKeywordAsObservable("runaway")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenCrazyDriving:" + IsActive, 3.0f);

                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            CreateTickAsObservable(3000)
                .Where(_ => IsActive)
                .Subscribe(_ => RunAway());
        }

        private void RunAway()
        {
            affectPeds.RemoveAll(x => !x.IsSafeExist());
            if (!playerPed.IsSafeExist()) return;

            var playerVehicle = playerPed.CurrentVehicle;

            //プレイヤ周辺の車
            var drivers = CachedVehicles.Where(x => x.IsSafeExist()
                                                    && (!playerVehicle.IsSafeExist() || !x.IsSameEntity(playerVehicle))
                                                    && !x.IsRequiredForMission()
                                                    && (x.Position - playerPed.Position).Length() <= PlayerAroundDistance)
                .Select(x => x.GetPedOnSeat(VehicleSeat.Driver))
                .Where(x => x.IsSafeExist() && !affectPeds.Contains(x));


            foreach (var driver in drivers)
            {
                try
                {
                    driver.DrivingSpeed = 100.0f;
                    driver.MaxDrivingSpeed = 100.0f;
                    driver.DrivingStyle = DrivingStyle.AvoidTrafficExtremely;
                    driver.Task.VehicleChase(playerPed);
                    affectPeds.Add(driver);
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
            }
        }
    }
}
