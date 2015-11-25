using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug]
    class Onakaitai : ParupunteScript
    {
        public Onakaitai(ParupunteCore core) : base(core)
        {
        }
        private ReduceCounter reduceCounter;
        public override string Name { get; } = "おなかいたい";
        public override void OnStart()
        {
            reduceCounter = new ReduceCounter(20000);
            AddProgressBar(reduceCounter);
            //コルーチン起動
            StartCoroutine(OilCoroutine());
        }

        public override void OnFinished()
        {
            reduceCounter.Finish();
        }

        private IEnumerable<object> OilCoroutine()
        {
            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            while (!reduceCounter.IsCompleted)
            {
                CreatePetro();
                yield return WaitForSeconds(1);
            }

            ParupunteEnd();
        }

        private void CreatePetro()
        {
            var player = core.PlayerPed;
            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_petrol",
                player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);
        }
    }
}
