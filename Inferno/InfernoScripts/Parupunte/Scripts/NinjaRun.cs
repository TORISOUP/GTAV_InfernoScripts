using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(true,true)]
    class NinjaRun : ParupunteScript
    {

        public NinjaRun(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "NINJA RUN";
        public override void OnStart()
        {

            this.UpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Subscribe(_ =>
                {
                    SetAnimRate(core.PlayerPed, 5.0f);
                    Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, core.PlayerPed, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0, 0,
                        0);
                    Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, core.PlayerPed, true);
                    var hp = core.PlayerPed.ForwardVector;
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, core.PlayerPed, hp.X * 1, hp.Y * 1, hp.Z * 1, 0, 0, 0, 1, false,
                        true, true, true, true);

                    Function.Call(Hash.TASK_PLAY_ANIM, core.PlayerPed, "move_m@generic", "sprint", 8.0, -8.0, -1, 9, 0, 0, 0,
                        0);
                });

            this.UpdateAsObservable
                .Select(_ => core.IsGamePadPressed(GameKey.Sprint))
                .DistinctUntilChanged()
                .Where(x => !x)
                .Subscribe(_ =>
                {

                    SetAnimRate(core.PlayerPed, 1);
                    Function.Call(Hash.TASK_FORCE_MOTION_STATE, core.PlayerPed, 0xFFF7E7A4, 0);
                });

        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped,(double) rate, 0.0, 0.0);
        }
    }
}
