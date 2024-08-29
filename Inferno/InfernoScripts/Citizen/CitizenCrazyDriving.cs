﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using GTA;

namespace Inferno
{
    /// <summary>
    /// 市民の運転スピード上限増加
    /// </summary>
    internal class CitizenCrazyDriving : InfernoScript
    {
        private readonly HashSet<Entity> _affectPeds = new();
        private readonly float PlayerAroundDistance = 300f;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("runaway")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenCrazyDriving:" + IsActive);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            CreateTickAsObservable(TimeSpan.FromSeconds(3))
                .Where(_ => IsActive)
                .Subscribe(_ => RunAway());
        }

        private void RunAway()
        {
            _affectPeds.RemoveWhere(x => !x.IsSafeExist());
            if (!PlayerPed.IsSafeExist())
            {
                return;
            }

            var playerVehicle = PlayerPed.CurrentVehicle;

            //プレイヤ周辺の車
            var drivers = CachedVehicles.Where(x => x.IsSafeExist()
                                                    && (!playerVehicle.IsSafeExist() || !x.IsSameEntity(playerVehicle))
                                                    && !x.IsRequiredForMission()
                                                    && (x.Position - PlayerPed.Position).Length() <=
                                                    PlayerAroundDistance)
                .Select(x => x.GetPedOnSeat(VehicleSeat.Driver))
                .Where(x => x.IsSafeExist() && !_affectPeds.Contains(x));

            foreach (var driver in drivers)
            {
                try
                {
                    driver.DrivingSpeed = 100.0f;
                    driver.MaxDrivingSpeed = 100.0f;
                    driver.DrivingStyle = DrivingStyle.AvoidTrafficExtremely;
                    driver.Task.VehicleChase(PlayerPed);
                    _affectPeds.Add(driver);
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
            }
        }
    }
}