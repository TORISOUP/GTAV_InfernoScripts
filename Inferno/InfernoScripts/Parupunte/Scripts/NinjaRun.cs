using System;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts;
using Inferno.InfernoScripts.Parupunte;

namespace Inferno
{
    [ParupunteConfigAttribute("バリキ・ジツ", "もうオシマイだ！")]
    [ParupunteIsono("ばりきじつ")]
    internal class NinjaRun : ParupunteScript
    {
        private float addSpeed = 1.0f;

        public NinjaRun(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

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
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, ptfxName);

            //メイン処理
            OnUpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Subscribe(_ =>
                {
                    SetAnimRate(core.PlayerPed, 6.0f + addSpeed);
                    Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, core.PlayerPed, 200000000.0, 1, 1000, 1, 0, 0, 0, 0,
                        0,
                        0, 0);
                    Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, core.PlayerPed, true);
                    var hp = core.PlayerPed.ForwardVector;
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, core.PlayerPed, hp.X * addSpeed, hp.Y * addSpeed,
                        hp.Z * addSpeed, 0, 0, 0, 1, false,
                        true, true, true, true);
                    Function.Call(Hash.TASK_PLAY_ANIM, core.PlayerPed, "move_m@generic", "sprint", 8.0, -8.0, -1, 9, 0,
                        0, 0, 0);

                    //徐々に加速
                    addSpeed *= 1.05f;
                    addSpeed = Math.Min(5, addSpeed);
                });

            //定期的にエフェクト再生
            OnUpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Sample(TimeSpan.FromSeconds(1))
                .Subscribe(_ => { StartFire(); });

            //左右移動
            OnUpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.Sprint))
                .Select(_ => core.GetStickValue().X)
                .Subscribe(input =>
                {
                    var player = core.PlayerPed;
                    player.Quaternion = Quaternion.RotationAxis(player.UpVector, -input / 127.0f * 0.2f) *
                                        player.Quaternion;
                });

            //ボタンを離したら中断
            OnUpdateAsObservable
                .Select(_ => core.IsGamePadPressed(GameKey.Sprint))
                .DistinctUntilChanged()
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    addSpeed = 1.0f;
                    SetAnimRate(core.PlayerPed, 1);
                    Function.Call(Hash.TASK_FORCE_MOTION_STATE, core.PlayerPed, 0xFFF7E7A4, 0);
                });

            //死んだら爆発
            OnUpdateAsObservable
                .Select(_ => core.PlayerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => { World.AddExplosion(core.PlayerPed.Position, GTA.ExplosionType.Rocket, 1.0f, 1.0f); });

            ReduceCounter = new ReduceCounter(25000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private int StartFire()
        {
            var player = core.PlayerPed;
            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var scale = 1.0f;

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_electrical_box",
                player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelLeftToe0, scale,
                0, 0, 0);

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_dst_elec_fire",
                player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelRightToe0, scale,
                0, 0, 0);
        }
    }
}