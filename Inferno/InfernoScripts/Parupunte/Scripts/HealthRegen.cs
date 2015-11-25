using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class HealthRegen : ParupunteScript
    {
        private ReduceCounter reduceCounter;
        public HealthRegen(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "リジェネ";

        private uint coroutineId = 0;
        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(30000);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            coroutineId = StartCoroutine(HealthRegenCoroutine());
            AddProgressBar(reduceCounter);
        }

        public override void OnFinished()
        {
            reduceCounter.Finish();
            StopCoroutine(coroutineId);
        }

        IEnumerable<object> HealthRegenCoroutine()
        {
            while (!reduceCounter.IsCompleted)
            {
                core.PlayerPed.Health += 8;
                yield return WaitForSeconds(1);
            }
        }

    }
}
