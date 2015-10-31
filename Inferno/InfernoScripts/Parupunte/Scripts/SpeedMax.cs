using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{

    [ParupunteDebug(true)]
    class SpeedMax:ParupunteScript
    {
        public SpeedMax(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "SpeedMax";


        private ReduceCounter reduceCounter;
        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(15 * 1000);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(reduceCounter);
        }

        public override void OnFinished()
        {
            reduceCounter.Finish();
        }

        public override void OnUpdate()
        {
            var radius = 50.0f;
            var player = core.PlayerPed;
            foreach (var vec in core.CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(player.Position,radius)
                ))
            {
                vec.Speed = vec.Handle%10 == 0 ? -300 : 300;
            }

        }
    }
}
