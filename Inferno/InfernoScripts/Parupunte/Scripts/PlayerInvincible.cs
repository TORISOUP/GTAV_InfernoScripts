using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class PlayerInvincible : ParupunteScript
    {
        private ReduceCounter reduceCounter;

        public PlayerInvincible(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "ムテキング";

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(30000);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(reduceCounter);
        }

        public override void OnFinished()
        {
            reduceCounter.Finish();
            core.PlayerPed.IsInvincible = false;
        }

        public override void OnUpdate()
        {
            core.PlayerPed.IsInvincible = true;
        }
    }
}
