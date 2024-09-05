using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts;
using Inferno.InfernoScripts.Parupunte;
using Inferno.Utilities;

namespace Inferno
{
    [ParupunteConfigAttribute("バリキ・ジツ", "もうオシマイだ！")]
    [ParupunteIsono("ばりきじつ")]
    internal class NinjaRun : ParupunteScript
    {
        private float addSpeed = 1.0f;
        private bool _isRunning = false;

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
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, ptfxName);

            //定期的にエフェクト再生
            OnUpdateAsObservable
                .Where(_ => _isRunning)
                .Sample(TimeSpan.FromSeconds(1), core.InfernoScheduler)
                .Subscribe(_ => StartFire());

            ReduceCounter = new ReduceCounter(25000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            MainLoopAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask MainLoopAsync(CancellationToken ct)
        {
            var isReseted = true;
            while (!ct.IsCancellationRequested)
            {
                if (!core.PlayerPed.IsSafeExist())
                {
                    await Delay100MsAsync(ct);
                    continue;
                }

                if (core.PlayerPed.IsDead)
                {
                    World.AddExplosion(core.PlayerPed.Position, GTA.ExplosionType.Rocket, 1.0f, 1.0f);
                    ParupunteEnd();
                    return;
                }

                if (core.IsGamePadPressed(GameKey.Sprint))
                {
                    if (isReseted)
                    {
                        Function.Call(Hash.TASK_PLAY_ANIM, core.PlayerPed, "move_m@generic", "sprint", 8.0, -8.0, -1, 9,
                            0,
                            0, 0, 0);
                        Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, core.PlayerPed, 200000000.0, 1, 1000, 1, 0, 0, 0,
                            0,
                            0,
                            0, 0);
                        Function.Call(Hash.SET_ACTIVATE_OBJECT_PHYSICS_AS_SOON_AS_IT_IS_UNFROZEN, core.PlayerPed, true);
                    }

                    isReseted = false;

                    var input = core.GetStickValue().X;
                    var player = core.PlayerPed;
                    player.Quaternion = Quaternion.RotationAxis(player.UpVector, -input / 127.0f * 0.2f) *
                                        player.Quaternion;

                    SetAnimRate(core.PlayerPed, 6.0f + addSpeed);

                    var hp = core.PlayerPed.ForwardVector;
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, core.PlayerPed, hp.X * addSpeed, hp.Y * addSpeed,
                        hp.Z * addSpeed, 0, 0, 0, 1, false,
                        true, true, true, true);


                    //徐々に加速
                    addSpeed *= 1.05f;
                    addSpeed = Math.Min(5, addSpeed);
                    _isRunning = true;
                }
                else
                {
                    if (!isReseted)
                    {
                        isReseted = true;
                        addSpeed = 1.0f;
                        SetAnimRate(core.PlayerPed, 1);
                        Function.Call(Hash.TASK_FORCE_MOTION_STATE, core.PlayerPed, 0xFFF7E7A4, 0);
                    }

                    _isRunning = false;
                }

                await Delay100MsAsync(ct);
            }
        }

        private void SetAnimRate(Ped ped, float rate)
        {
            Function.Call(Hash.SET_ANIM_RATE, ped, (double)rate, 0.0, 0.0);
        }

        private void StartFire()
        {
            var player = core.PlayerPed;
            if (!player.IsSafeExist())
            {
                return;
            }

            var offset = new Vector3(0.0f, 0.0f, 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var scale = 1.0f;

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_electrical_box",
                player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelLeftToe0, scale,
                0, 0, 0);

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, "ent_sht_electrical_box",
                player, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelRightToe0,
                scale,
                0, 0, 0);
        }
    }
}