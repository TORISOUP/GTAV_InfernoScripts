using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class ExplodeDeadBodies : ParupunteScript
    {
        private ReduceCounter reduceCounter;
        private HashSet<int> explodedPedHandles;

        public ExplodeDeadBodies(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "ば・く・は・つ・し・た・い";

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(20000);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(reduceCounter);
            explodedPedHandles = new HashSet<int>();
        }

        public override void OnFinished()
        {
            reduceCounter.Finish();
        }

        public override void OnUpdate()
        {
            explodedPedHandles.RemoveWhere(x => !Function.Call<bool>(Hash.DOES_ENTITY_EXIST, x));//後の誤判定防止のため;

            var player = core.PlayerPed;
            var peds = core.CachedPeds.Where(x => x.IsSafeExist()
                                                && !x.IsSameEntity(core.PlayerPed)
                                                && x.IsDead
                                                && !explodedPedHandles.Contains(x.Handle));

            foreach (var ped in peds)
            {
                var killer = ped.GetKiller();
                if (killer.IsSafeExist() && killer is Ped)//殺害者がいたらそいつが起こした扱いの爆発を生成
                {
                    GTA.World.AddOwnedExplosion((Ped)killer, ped.Position, GTA.ExplosionType.Rocket, 8.0f, 2.5f);
                }
                else
                {
                    GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Rocket, 8.0f, 2.5f);
                }
                explodedPedHandles.Add(ped.Handle);//同一人物が2回以上爆発するのを防止するため
            }
        }
    }
}
