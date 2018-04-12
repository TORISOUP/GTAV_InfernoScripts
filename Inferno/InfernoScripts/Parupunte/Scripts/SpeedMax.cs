using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("すぴーどまっくす")]
    internal class SpeedMax : ParupunteScript
    {
        public SpeedMax(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; } = new ParupunteConfigElement("光速進行", "法定速度を守ってマナーよく運転しましょう");

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
                x => x.IsSafeExist() && x.IsInRangeOf(player.Position, radius)
                ))
            {
                vec.Speed = vec.Handle % 10 == 0 ? -200 : 200;
            }
        }
    }
}
