﻿using GTA.Math;
using GTA.Native;
using System.Linq;
using System;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("ふわふわ", "おわり")]
    [ParupunteIsono("ふわふわ")]
    internal class FloatingCar : ParupunteScript
    {
        public FloatingCar(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }

        protected override void OnUpdate()
        {
            var player = core.PlayerPed;
            var targets = core.CachedVehicles
                .Where(x => x.IsSafeExist()
                            && x.IsInRangeOf(player.Position, 50.0f)
                            && x != player.CurrentVehicle
                            && !x.IsPersistent
                );
            foreach (var vehicle in targets)
            {
                vehicle.ApplyForce(Vector3.WorldUp);
            }

            if (player.IsInVehicle() && player.CurrentVehicle.IsSafeExist())
            {
                var v = player.CurrentVehicle;
                if (Function.Call<bool>(Hash.IS_VEHICLE_ON_ALL_WHEELS, v))
                {
                    v.ApplyForce(Vector3.WorldUp * 1.2f);
                }
            }
        }
    }
}
