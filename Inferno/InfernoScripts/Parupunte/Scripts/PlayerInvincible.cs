using System;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class PlayerInvincible : ParupunteScript
    {
        private ReduceCounter reduceCounter;

        public PlayerInvincible(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "無敵";

        public override void OnSetUp()
        {
            
        }

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(15000);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(reduceCounter);
            StartCoroutine(Coroutine());
        }

        private IEnumerable<object> Coroutine()
        {
            while (!reduceCounter.IsCompleted)
            {
                if (!core.PlayerPed.IsInvincible)
                {
                    core.PlayerPed.IsInvincible = true;
                }
                yield return WaitForSeconds(1);
            }
        }

        protected override void OnFinished()
        {
            reduceCounter.Finish();
            core.PlayerPed.IsInvincible = false;
            core.DrawParupunteText("おわり",3.0f);
        }

        protected override void OnUpdate()
        {
            core.PlayerPed.IsInvincible = true;
        }
    }
}
