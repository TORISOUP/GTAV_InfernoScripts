using System;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("無敵", "おわり")]
    [ParupunteIsono("むてき")]
    internal class PlayerInvincible : ParupunteScript
    {
        public PlayerInvincible(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15000);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(ReduceCounter);
            StartCoroutine(Coroutine());
        }

        private IEnumerable<object> Coroutine()
        {
            while (!ReduceCounter.IsCompleted)
            {
                if (!core.PlayerPed.IsInvincible) core.PlayerPed.IsInvincible = true;
                yield return WaitForSeconds(1);
            }
        }

        protected override void OnFinished()
        {
            core.PlayerPed.IsInvincible = false;
        }

        protected override void OnUpdate()
        {
            core.PlayerPed.IsInvincible = true;
        }
    }
}