using System;
using System.Linq;
using GTA;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    //[ParupunteConfigAttribute("変身GOGOベイビー")]
    [ParupunteDebug(false, true)]
    internal class Transform : ParupunteScript
    {
        public Transform(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(30 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            var initialModel = core.PlayerPed.Model;

            var hashed = Enum.GetValues(typeof(PedHash)).Cast<PedHash>().ToArray();
            var targetHash = hashed[Random.Next(hashed.Length)];
            var targetModel = new Model(targetHash);

            Game.Player.ChangeModel(targetModel);
            OnFinishedAsObservable
                .Subscribe(_ => Game.Player.ChangeModel(initialModel));
        }
    }
}