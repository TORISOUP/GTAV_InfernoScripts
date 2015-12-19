﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class FloatingCar : ParupunteScript
    {
        private ReduceCounter reduceCounter;

        public FloatingCar(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "ふわふわ";
        public override string EndMessage { get; } = "おわり";

        public override void OnSetUp()
        {

        }

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(20*1000);
            AddProgressBar(reduceCounter);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            this.UpdateAsObservable
                .Subscribe(_ =>
                {
                    var player = core.PlayerPed;
                    var targets = core.CachedVehicles
                        .Where(x => x.IsSafeExist()
                                    && x.IsInRangeOf(player.Position, 80.0f)
                                    && x != player.CurrentVehicle

                        );
                    foreach (var vehicle in targets)
                    {
                        vehicle.ApplyForce(Vector3.WorldUp);
                    }

                    if (core.PlayerPed.IsInVehicle())
                    {
                        var v = core.PlayerPed.CurrentVehicle;
                        if (Function.Call<bool>(Hash.IS_VEHICLE_ON_ALL_WHEELS, v))
                        {
                            v.ApplyForce(Vector3.WorldUp);
                        }
                    }
                });

        }
    }
}
