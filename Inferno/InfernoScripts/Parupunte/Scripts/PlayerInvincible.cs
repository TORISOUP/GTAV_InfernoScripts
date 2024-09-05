using System;

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
        }

        protected override void OnFinished()
        {
            var p = core.PlayerPed;
            if (p.IsSafeExist())
            {
                p.IsInvincible = false;
            }
        }

        protected override void OnUpdate()
        {
            var p = core.PlayerPed;
            if (p.IsSafeExist())
            {
                p.IsInvincible = true;
            }
        }
    }
}