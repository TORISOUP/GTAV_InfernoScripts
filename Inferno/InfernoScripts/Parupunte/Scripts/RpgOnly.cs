using System;
using Inferno.InfernoScripts.Event.ChasoMode;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("RPG ONLY", "おわり")]
    [ParupunteIsono("あーるぴーじー")]
    internal class RpgOnly : ParupunteScript
    {
        public RpgOnly(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            OnFinishedAsObservable.Subscribe(_ => { Inferno.InfernoCore.Publish(ChasoModeEvent.SetToDefault); });

            Inferno.InfernoCore.Publish(new ChangeWeaponEvent(Weapon.RPG));
        }
    }
}