using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// 尻から炎
    /// </summary>
    [ParupunteConfigAttribute("ただし魔法は尻から出る", "　お　し　り　")]
    [ParupunteIsono("おしり")]
    internal class MagicFire : ParupunteScript
    {
        private uint coroutineId;

        public MagicFire(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20000);
            AddProgressBar(ReduceCounter);
            //コルーチン起動
            coroutineId = StartCoroutine(MagicFireCoroutine());
        }

        protected override void OnFinished()
        {
            StopCoroutine(coroutineId);
            //終了時に炎耐性解除
            core.PlayerPed.IsFireProof = false;
        }

        private IEnumerable<object> MagicFireCoroutine()
        {
            var ptfxName = "core";

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, ptfxName);

            while (!ReduceCounter.IsCompleted)
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
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");

            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_flame",
                player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale,
                0, 0, 0);
        }
    }
}