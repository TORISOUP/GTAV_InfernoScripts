using System;
using System.Collections.Generic;
using UniRx;
namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class PlayerInvincible : ParupunteScript
    {

        public PlayerInvincible(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "無敵";
        public override string EndMessage => "おわり";
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
                if (!core.PlayerPed.IsInvincible)
                {
                    core.PlayerPed.IsInvincible = true;
                }
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
