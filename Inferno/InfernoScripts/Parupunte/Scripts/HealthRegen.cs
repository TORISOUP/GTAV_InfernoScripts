using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GTA; using UniRx;
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
        public override string EndMessage => "おわり";
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

        IEnumerable<object> HealthRegenCoroutine()
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
