using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(true)]
    class MagicFire : ParupunteScript
    {
        private ReduceCounter reduceCounter;
        public MagicFire(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "ただし魔法は尻から出る";
        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(5000);
            AddProgressBar(reduceCounter);
            //コルーチン起動
            StartCoroutine(MagicFireCoroutine());

 
        }

        IEnumerable<object> MagicFireCoroutine()
        {
            var player = core.PlayerPed;
            var ptfx = "core";
            while (!reduceCounter.IsCompleted)
            {

                if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfx))
                {
                    Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfx);
                }
                Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfx);

               Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_flame",
                        player.Handle, -0.02, 0.2, 0.0, 90.0,100.0, 90.0, 31086, 1, 0, 0, 0);
                

                yield return null;
            }

            ParupunteEnd();
        }
    }
}
