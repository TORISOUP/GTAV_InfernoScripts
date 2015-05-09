﻿using System;
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
    class CitizenCrazyDriving : InfernoScript
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
            var drivers = CachedVehicles.Where(x => x.IsSafeExist()
                                                   && !x.IsSameEntity(this.GetPlayerVehicle())
                                                   && !x.IsRequiredForMission()
                                                   && (x.Position - player.Position).Length() <= PlayerAroundDistance)
                                           .Select(x => x.GetPedOnSeat(VehicleSeat.Driver))
                                           .Where(x => x.IsSafeExist());


            foreach (var driver in drivers)
            {
                try
                {
                    driver.SetMaxDriveSpeed(300.0f);
                    driver.SetDriveSpeed(300.0f);
                }
                catch 
                {
                    //nice catch!
                }
            }
        }
    }
}
