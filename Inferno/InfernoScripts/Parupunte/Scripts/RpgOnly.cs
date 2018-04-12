using Inferno.InfernoScripts.Event.ChasoMode;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("あーるぴーじー")]
    class RpgOnly : ParupunteScript
    {
        public RpgOnly(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; }
            = new ParupunteConfigElement("RPG ONLY", "おわり");

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            this.OnFinishedAsObservable.Subscribe(_ =>
            {
                Inferno.InfernoCore.Publish(ChasoModeEvent.SetToDefault);
            });

            Inferno.InfernoCore.Publish(new ChangeWeaponEvent(Weapon.RPG));

        }
    }
}
