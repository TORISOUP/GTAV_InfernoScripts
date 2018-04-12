using System.Collections.Generic;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class HealthRegen : ParupunteScript
    {
        private ReduceCounter reduceCounter;

        public HealthRegen(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; }
            = new ParupunteConfigElement("リジェネ", "おわり");

        private uint coroutineId = 0;

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(10000);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            coroutineId = StartCoroutine(HealthRegenCoroutine());
            AddProgressBar(reduceCounter);
        }

        protected override void OnFinished()
        {
            reduceCounter.Finish();
            StopCoroutine(coroutineId);
        }

        private IEnumerable<object> HealthRegenCoroutine()
        {
            while (!reduceCounter.IsCompleted)
            {
                if (core.PlayerPed.Health < core.PlayerPed.MaxHealth)
                {
                    core.PlayerPed.Health += 15;
                }
                else
                {
                    core.PlayerPed.Armor += 20;
                }
                yield return WaitForSeconds(1);
            }
        }
    }
}
