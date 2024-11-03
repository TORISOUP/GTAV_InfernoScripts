using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("リジェネ", "おわり")]
    internal class HealthRegen : ParupunteScript
    {
        private ReduceCounter _reduceCounter;

        public HealthRegen(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            _reduceCounter = new ReduceCounter(10000);
            _reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            HealthRegenAsync(ActiveCancellationToken).Forget();
            AddProgressBar(_reduceCounter);
        }

        protected override void OnFinished()
        {
            _reduceCounter.Finish();
        }

        private async ValueTask HealthRegenAsync(CancellationToken ct)
        {
            while (!_reduceCounter.IsCompleted)
            {
                if (core.PlayerPed.Health < core.PlayerPed.MaxHealth)
                {
                    core.PlayerPed.Health += 15;
                }
                else
                {
                    core.PlayerPed.Armor += 20;
                }

                await DelaySecondsAsync(1, ct);
            }
        }
    }
}