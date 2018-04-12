using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("えどじだい")]
    internal class CitizenNinja : ParupunteScript
    {
        private Random random = new Random();
        private HashSet<int> ninjas = new HashSet<int>();
        private List<Ped> pedList = new List<Ped>();

        public CitizenNinja(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; } = new ParupunteConfigElement("江　戸　時　代", "　現　代　");
        
        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20000);
            AddProgressBar(ReduceCounter);

            var ptfxName = "core";

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);

            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(core.PlayerPed.Position, 100) && x.IsAlive))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                ped.Task.ClearAllImmediately();
                StartCoroutine(DashCoroutine(ped));
            }

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                foreach (var ped in pedList)
                {
                    if (ped.IsSafeExist())
                    {
                        SetAnimRate(ped, 1);
                        Function.Call(Hash.TASK_FORCE_MOTION_STATE, ped, 0xFFF7E7A4, 0);
                    }
                }

                ParupunteEnd();
            });
        }

        protected override void OnUpdate()
        {
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive
                                                           && x.IsInRangeOf(core.PlayerPed.Position, 100) &&
                                                           !ninjas.Contains(x.Handle)))
            {
                pedList.Add(ped);
                ninjas.Add(ped.Handle);
                ped.Task.ClearAllImmediately();
                StartCoroutine(DashCoroutine(ped));
            }
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private IEnumerable<object> DashCoroutine(Ped ped)
        {
            while (!ReduceCounter.IsCompleted)
            {
                if (!ped.IsSafeExist()) yield break;
                if (ped.IsDead)
                {
                    GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Rocket, 1.0f, 1.0f);
                    yield break;
                }

                if (random.Next(100) < 10)
                {
                    ped.Quaternion = Quaternion.RotationAxis(ped.UpVector, (float)(random.NextDouble() - 0.5)) * ped.Quaternion;
                }

                SetAnimRate(ped, 5.0f);
                Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, ped, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0,
                    0, 0);
                Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, ped, true);
                var hp = core.PlayerPed.ForwardVector;
                Function.Call(Hash.APPLY_FORCE_TO_ENTITY, ped, hp.X * 1, hp.Y * 1, hp.Z * 1, 0, 0, 0, 1, false,
                    true, true, true, true);
                Function.Call(Hash.TASK_PLAY_ANIM, ped, "move_m@generic", "sprint", 8.0, -8.0, -1, 9, 0,
                    0, 0, 0);
                StartFire(ped);

                yield return null;
            }
        }

        private void StartFire(Ped ped)
        {
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var scale = 2.0f;

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_electrical_box",
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_L_Toe0, scale,
                0, 0, 0);

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_dst_elec_fire",
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_R_Toe0, scale,
                0, 0, 0);
        }
    }
}
