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
    class DeadBodyBombs : ParupunteScript
    {
        private ReduceCounter reduceCounter;

        public DeadBodyBombs(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "死体が爆発するぞ！気をつけろ！";

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(20000);
            reduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(reduceCounter);
        }

        public override void OnFinished()
        {
            reduceCounter.Finish();
        }

        HashSet<int> explodedPedHandles = new HashSet<int>();

        public override void OnUpdate()
        {
            var player = core.PlayerPed;

            var peds = core.CachedPeds.Where(
                x => x.IsSafeExist() && !x.IsSameEntity(core.PlayerPed) && !x.IsRequiredForMission());

            explodedPedHandles.Where(x => Function.Call<bool>(Hash.DOES_ENTITY_EXIST, x));

            foreach (var ped in peds)
            {
                if (explodedPedHandles.Contains(ped.Handle)) { continue; }

                if (ped.IsDead)
                {
                    var killer = ped.GetKiller();

                    if (killer.IsSafeExist())
                    {
                        GTA.World.AddOwnedExplosion(ped, ped.Position, GTA.ExplosionType.Rocket, 8.0f, 2.5f);
                    }
                    else
                    {
                        GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Rocket, 8.0f, 2.5f);
                    }

                    if (ped.IsRequiredForMission())
                    {
                        explodedPedHandles.Add(ped.Handle);
                        ped.IsVisible = false;
                    }
                    else
                    {
                        ped.Delete();
                    }

                }
            }
        }
    }
}
