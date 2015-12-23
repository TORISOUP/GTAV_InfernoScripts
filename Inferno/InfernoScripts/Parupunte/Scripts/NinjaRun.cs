using System;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts.Parupunte;

namespace Inferno
{
    class NinjaRun : ParupunteScript
    {
        public NinjaRun(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "バリキ・ジツ";
        public override string EndMessage => "もうオシマイだ！";
        public override void OnSetUp()
        {
            
        }

        protected override void OnFinished()
        {
            SetAnimRate(core.PlayerPed, 1);
            Function.Call(Hash.TASK_FORCE_MOTION_STATE, core.PlayerPed, 0xFFF7E7A4, 0);
        }

        public override void OnStart()
        {
            var ptfxName = "core";

            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, ptfxName);

            core.OnTickAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Subscribe(_ =>
                {
                    SetAnimRate(core.PlayerPed, 5.0f);
                    Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, core.PlayerPed, 200000000.0, 1, 1000, 1, 0, 0, 0, 0, 0, 0,0);
                    Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, core.PlayerPed, true);
                    var hp = core.PlayerPed.ForwardVector;
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, core.PlayerPed, hp.X * 1, hp.Y * 1, hp.Z * 1, 0, 0, 0, 1, false,true, true, true, true);
                    Function.Call(Hash.TASK_PLAY_ANIM, core.PlayerPed, "move_m@generic", "sprint", 8.0, -8.0, -1, 9, 0, 0, 0,0);

                    StartFire();
                });

            var num = 0;
            core.OnTickAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint) && num % 10 ==0)
                .Subscribe(_ =>
                {
                    num++;
                    StartFire();
                });

            core.OnTickAsObservable
                    .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Select(_ => core.GetStickValue().X)
                .Subscribe(input =>
                {
                    var player = core.PlayerPed;
                    player.Quaternion = Quaternion.RotationAxis(player.UpVector, (-input/127.0f) * 0.2f) * player.Quaternion;
                });

            core.OnTickAsObservable
                .Select(_ => core.IsGamePadPressed(GameKey.Sprint))
                .DistinctUntilChanged()
                .Where(x => !x)
                .Subscribe(_ =>
                {

                    SetAnimRate(core.PlayerPed, 1);
                    Function.Call(Hash.TASK_FORCE_MOTION_STATE, core.PlayerPed, 0xFFF7E7A4, 0);
                });
            core.OnTickAsObservable
                .Select(_ => core.PlayerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x).Subscribe(_ =>
                {
                    GTA.World.AddExplosion(core.PlayerPed.Position, GTA.ExplosionType.Rocket, 1.0f, 1.0f);
                });

            ReduceCounter = new ReduceCounter(20000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped,(double) rate, 0.0, 0.0);
        }

        private int StartFire()
        {
            var player = core.PlayerPed;
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var scale = 1.0f;

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
             Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_electrical_box",
                    player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_L_Toe0, scale, 0, 0, 0);

            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_dst_elec_fire",
                    player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_R_Toe0, scale, 0, 0, 0);
        }
    }
}
