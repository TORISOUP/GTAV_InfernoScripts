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
    /// <summary>
    /// 尻から炎
    /// </summary>
    [ParupunteDebug]
    class MagicFire : ParupunteScript
    {
        private ReduceCounter reduceCounter;
        public MagicFire(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "ただし魔法は尻から出る";
        public override string EndMessage => "　お　し　り　";
        private uint coroutineId = 0;
        public override void OnSetUp()
        {
            
        }

        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(20000);
            AddProgressBar(reduceCounter);
            //コルーチン起動
            coroutineId = StartCoroutine(MagicFireCoroutine());
        }

        protected override void OnFinished()
        {
            reduceCounter.Finish();
            StopCoroutine(coroutineId);
            //終了時に炎耐性解除
            core.PlayerPed.IsFireProof = false;
        }

        IEnumerable<object> MagicFireCoroutine()
        {
           
            var ptfxName = "core";

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);

            while (!reduceCounter.IsCompleted)
            {
                core.PlayerPed.IsFireProof = true;
                StartFire();
                yield return WaitForSeconds(1);
            }

            //まだ炎が残っているのでロスタイム
            yield return WaitForSeconds(3);

            ParupunteEnd();
        }

        private int StartFire()
        {
            var player = core.PlayerPed;
            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");

            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_flame",
                    player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);
        }

    }
}
