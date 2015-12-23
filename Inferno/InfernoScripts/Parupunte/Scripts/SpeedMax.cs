using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{

    [ParupunteDebug]
    class SpeedMax:ParupunteScript
    {
        public SpeedMax(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "光速進行";
        public override string EndMessage => "法定速度を守ってマナーよく運転しましょう";

        public override void OnSetUp()
        {
            
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(30 * 1000);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(ReduceCounter);
        }


        protected override void OnUpdate()
        {
            var radius = 50.0f;
            var player = core.PlayerPed;
            foreach (var vec in core.CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(player.Position,radius)
                ))
            {
                vec.Speed = vec.Handle%10 == 0 ? -200 : 200;
            }

        }
    }
}
