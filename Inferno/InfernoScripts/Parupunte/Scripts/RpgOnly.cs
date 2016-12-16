using Inferno.InfernoScripts.Event.ChasoMode;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class RpgOnly : ParupunteScript
    {
        public RpgOnly(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "RPG ONLY";
        public override string EndMessage { get; } = "おわり";

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            this.OnFinishedAsObservable.Subscribe(_ =>
            {
                InfernoCore.Publish(ChasoModeEvent.SetToDefault);
            });

            InfernoCore.Publish(new ChangeWeaponEvent(Weapon.RPG));

        }
    }
}
