﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("おなら")]
    internal class Fart : ParupunteScript
    {
        public Fart(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnStart()
        {
            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            FartAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask FartAsync(CancellationToken ct)
        {
            core.DrawParupunteText("3", 1.0f);
            await DelaySecondsAsync(1, ct);
            core.DrawParupunteText("2", 1.0f);
            await DelaySecondsAsync(1, ct);
            core.DrawParupunteText("1", 1.0f);
            await DelaySecondsAsync(1, ct);

            core.DrawParupunteText("発射！", 3.0f);
            GasExplosion();
            CreateEffect(core.PlayerPed, "ent_sht_steam");

            if (core.PlayerPed.IsInVehicle())
            {
                core.PlayerPed.CurrentVehicle.SetForwardSpeed(300);
            }

            core.PlayerPed.SetToRagdoll(10);
            core.PlayerPed.ApplyForce(Vector3.WorldUp * 10.0f);

            ParupunteEnd();
        }

        private void GasExplosion()
        {
            var playerPos = core.PlayerPed.Position;
            var targets = core.CachedPeds.Cast<Entity>()
                .Concat(core.CachedVehicles)
                .Where(x => x.IsSafeExist() && x.IsInRangeOf(playerPos, 400));


            Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
            {
                playerPos.X,
                playerPos.Y,
                playerPos.Z,
                -1,
                0.5f,
                true,
                false,
                0.5f
            });

            foreach (var e in targets)
            {
                var dir = (e.Position - playerPos).Normalized;
                e.ApplyForce(dir * 1000.0f, Vector3.Zero, ForceType.MaxForceRot);
            }
        }

        private void CreateEffect(Ped ped, string effect)
        {
            if (!ped.IsSafeExist())
            {
                return;
            }

            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, effect,
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelPelvis, scale, 0,
                0, 0);
        }
    }
}