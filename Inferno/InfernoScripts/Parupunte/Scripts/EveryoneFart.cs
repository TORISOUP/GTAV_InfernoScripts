using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("みんなおなら")]
    internal class EveryoneFart : ParupunteScript
    {
        public EveryoneFart(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
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

            core.DrawParupunteText("みんなで 発射！", 3.0f);

            var player = core.PlayerPed;
            var playerPos = player.Position;

            player.IsCollisionProof = true;
            try
            {
                var peds = core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(playerPos, 15f))
                    .Concat(new[] { player })
                    .ToArray();

                foreach (var ped in peds.Where(x => x.IsSafeExist()))
                {
                    if (!ped.IsSafeExist()) continue;
                    GasExplosion(ped);
                    CreateEffect(ped, "ent_sht_steam");

                    if (ped.IsInVehicle() && ped.CurrentVehicle.IsSafeExist())
                    {
                        if (ped != core.PlayerPed)
                        {
                            ped.Task.LeaveVehicle(ped.CurrentVehicle, LeaveVehicleFlags.WarpOut);
                            ped.Position += new Vector3(0, 0, 2.0f);
                        }
                        else
                        {
                            ped.CurrentVehicle.SetForwardSpeed(100);
                        }
                    }

                    if (!ped.IsCutsceneOnlyPed())
                    {
                        ped.Task.ClearAllImmediately();
                        ped.SetToRagdoll(500);
                    }

                    ped.ApplyForce(Vector3.WorldUp * 30.0f);

                    await DelaySecondsAsync(0.05f, ct);
                }

                await DelaySecondsAsync(3.0f, ct);
            }
            finally
            {
                core.PlayerPed.IsCollisionProof = false;
                ParupunteEnd();
            }
        }

        private void GasExplosion(Ped shotPed)
        {
            var playerPos = shotPed.Position;
            var targets = core.CachedPeds.Cast<Entity>()
                .Concat(core.CachedVehicles)
                .Concat(new[] { core.PlayerPed })
                .Where(x => x.IsSafeExist() && x.IsInRangeOf(shotPed.Position, 10));

            Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
            {
                playerPos.X,
                playerPos.Y,
                playerPos.Z,
                0,
                0.5f,
                true,
                false,
                0.5f
            });

            foreach (var e in targets)
            {
                if (e == shotPed) continue;
                if (!e.IsSafeExist()) continue;
                var dir = (e.Position - shotPed.Position).Normalized;

                if (e is Ped ped)
                {
                    if (ped.IsCutsceneOnlyPed())
                    {
                        ped.Task.ClearAll();
                        ped.SetToRagdoll(16);
                    }
                }

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