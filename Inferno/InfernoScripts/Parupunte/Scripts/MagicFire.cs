using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    /// <summary>
    /// 尻から炎
    /// </summary>
    [ParupunteConfigAttribute("ただし魔法は尻から出る", "　お　し　り　")]
    [ParupunteIsono("おしり")]
    internal class MagicFire : ParupunteScript
    {
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
            MagicFireAsync(ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            //終了時に炎耐性解除
            core.PlayerPed.IsFireProof = false;
        }

        private async ValueTask MagicFireAsync(CancellationToken ct)
        {
            var ptfxName = "core";

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, ptfxName);

            while (!ReduceCounter.IsCompleted)
            {
                core.PlayerPed.IsFireProof = true;
                StartFire();
                await DelaySecondsAsync(1, ct);
            }

            //まだ炎が残っているのでロスタイム
            await DelaySecondsAsync(3, ct);

            ParupunteEnd();
        }

        private void StartFire()
        {
            var player = core.PlayerPed;
            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");

            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_flame",
                player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelPelvis, scale,
                0, 0, 0);
        }
    }
}