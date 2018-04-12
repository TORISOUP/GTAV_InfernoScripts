using GTA;
using GTA.Native;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("ばくはつしたい")]
    internal class ExplodeDeadBodies : ParupunteScript
    {
        private HashSet<int> explodedPedHandles;

        public ExplodeDeadBodies(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; }
            = new ParupunteConfigElement("ば・く・は・つ・し・た・い！", "ば・く・は・つ・し・な・い");

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20000);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(ReduceCounter);
            explodedPedHandles = new HashSet<int>();
        }

        protected override void OnUpdate()
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
