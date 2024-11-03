using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("ふわふわ", "おわり")]
    [ParupunteIsono("ふわふわ")]
    internal class FloatingCar : ParupunteScript
    {
        private readonly List<Vehicle> _targetList = new();

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

            Function.Call(Hash.SET_GRAVITY_LEVEL, 1);
            UpdateAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask UpdateAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var player = core.PlayerPed;
                foreach (var vehicle in core.CachedVehicles
                             .Where(x => x.IsSafeExist()
                                         && !_targetList.Contains(x)
                                         && x.IsInRangeOf(player.Position, 150.0f)
                                         && !x.IsPersistent))
                {
                    _targetList.Add(vehicle);
                }

                await DelaySecondsAsync(1, ct);
            }
        }

        protected override void OnUpdate()
        {
            foreach (var v in _targetList)
            {
                if (v.IsSafeExist() && v.IsOnAllWheels)
                {
                    v.ApplyForce(Vector3.WorldUp * (float)Random.NextDouble() * 1.5f);
                }
            }
        }

        protected override void OnFinished()
        {
            Function.Call(Hash.SET_GRAVITY_LEVEL, 0);
            _targetList.Clear();
        }
    }
}