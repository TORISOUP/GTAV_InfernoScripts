using System;
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
    [ParupunteConfigAttribute("参　勤　交　代", "大　政　奉　還")]
    [ParupunteIsono("さんきんこうたい")]
    internal class MultiLeggedRace : ParupunteScript
    {
        private List<Ped> _allPedList = new();

        public MultiLeggedRace(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 500);
            AddProgressBar(ReduceCounter);

            OnUpdateAsObservable
                .Subscribe(_ =>
                {
                    if (core.PlayerPed.IsDead)
                    {
                        ParupunteEnd();
                    }
                });

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                foreach (var ped in _allPedList)
                {
                    if (ped.IsSafeExist())
                    {
                        GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Grenade, 1.0f, 0.0f);
                        ped.Kill();
                    }
                }

                ParupunteEnd();
            });

            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, ptfxName);

            _allPedList =
                core.CachedPeds.Where(x =>
                        x.IsSafeExist() && !x.IsRequiredForMission() && !x.IsCutsceneOnlyPed() && x.IsAlive)
                    .Take(50)
                    .ToList();

            var playerPos = core.PlayerPed.Position;
            var num = 10;

            var i = -_allPedList.Count / 2;
            foreach (var p in _allPedList.Where(x => x.IsSafeExist()))
            {
                p.Task.ClearAllImmediately();
                p.Position =
                    playerPos
                    + core.PlayerPed.RightVector * 2.5f * (i / num)
                    + core.PlayerPed.ForwardVector * (Math.Abs(i) % num)
                    + core.PlayerPed.ForwardVector * 5;

                if (p.IsSafeExist())
                {
                    AutoReleaseOnParupunteEnd(p);
                    p.Rotation = Quaternion.RotationAxis(core.PlayerPed.UpVector, (float)Math.PI) *
                                 core.PlayerPed.Rotation;
                    p.Health = 100;
                    DashAsync(p, ActiveCancellationToken).Forget();
                    i++;
                }
            }
        }

        protected override void OnFinished()
        {
            _allPedList.Clear();
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private async ValueTask DashAsync(Ped ped, CancellationToken ct)
        {
            var speed = 5;
            var isReturedCount = 10;
            while (!ReduceCounter.IsCompleted && !ct.IsCancellationRequested)
            {
                if (!ped.IsSafeExist())
                {
                    return;
                }

                if (ped.IsDead)
                {
                    GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Grenade, 1.0f, 0.2f);
                    return;
                }

                if (isReturedCount == 0)
                {
                    isReturedCount = 10;
                    ped.Quaternion = Quaternion.RotationAxis(ped.UpVector, (float)Math.PI) * ped.Quaternion;
                }
                
                if (isReturedCount > 0)
                {
                    isReturedCount--;
                }

                SetAnimRate(ped, speed);
                Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, ped, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                    0, 0);
                Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, ped, true);
                var hp = core.PlayerPed.ForwardVector;
                Function.Call(Hash.APPLY_FORCE_TO_ENTITY, ped, hp.X * 1, hp.Y * 1, hp.Z * 1, 0, 0, 0, 1, false,
                    true, true, true, true);
                Function.Call(Hash.TASK_PLAY_ANIM, ped, "move_m@generic", "sprint", 8.0, -8.0, -1, 9, 0,
                    0, 0, 0);
                StartFire(ped);

                await Delay100MsAsync(ct);
            }
        }

        private void StartFire(Ped ped)
        {
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var scale = 2.0f;

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_electrical_box",
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelLeftToe0, scale,
                0, 0, 0);

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_dst_elec_fire",
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelRightToe0, scale,
                0, 0, 0);
        }
    }
}