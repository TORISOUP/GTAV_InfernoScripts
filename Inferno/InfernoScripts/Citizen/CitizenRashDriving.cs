using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using GTA;
using GTA.Native;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;

namespace Inferno
{
    /// <summary>
    /// 市民の運転スピード上限増加
    /// </summary>
    internal class CitizenRashDriving : InfernoScript
    {
        private readonly HashSet<Entity> _affectPeds = new();
        private readonly float PlayerAroundDistance = 300f;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("RashDriving", "runaway")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("RashDriving:" + IsActive);
                });


            CreateTickAsObservable(TimeSpan.FromSeconds(1))
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
            var drivers = CachedPeds.Where(x => x.IsSafeExist()
                                                && (!playerVehicle.IsSafeExist() || !x.IsSameEntity(playerVehicle))
                                                && !x.IsRequiredForMission()
                                                && x.IsInRangeOfIgnoreZ(PlayerPed.Position, PlayerAroundDistance)
                                                && !_affectPeds.Contains(x)
                                                && x.IsInVehicle() && x.SeatIndex == VehicleSeat.Driver);

            foreach (var driver in drivers)
            {
                try
                {
                    driver.DrivingSpeed = 200.0f;
                    driver.MaxDrivingSpeed = 200.0f;
                    Function.Call(Hash.SET_DRIVE_TASK_DRIVING_STYLE, driver,
                        64 | 256 | 512 | 2048 | 262144 | 16777216 | 1073741824 | 524288);
                    _affectPeds.Add(driver);
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
            }
        }

        public override bool UseUI => true;
        public override string DisplayName => EntitiesLocalize.CrazyDrivingTitle;

        public override string Description => EntitiesLocalize.CrazyDrivingDescription;

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Entities;
    }
}