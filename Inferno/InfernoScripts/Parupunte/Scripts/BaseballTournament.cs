using Inferno.InfernoScripts.Event.ChasoMode;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// 野球大会
    /// </summary>
    [ParupunteConfigAttribute("野球大会", "おわり")]
    [ParupunteIsono("やきゅう")]
    class BaseballTournament : ParupunteScript
    {
        public BaseballTournament(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            this.OnFinishedAsObservable.Subscribe(_ =>
            {
                Inferno.InfernoCore.Publish(ChasoModeEvent.SetToDefault);
            });

            Inferno.InfernoCore.Publish(new ChangeWeaponEvent(Weapon.BAT));
        }
    }
}
