using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("行き先を選べ!")]
    [ParupunteIsono("けつるーら")]
    internal class KetsuWarp : ParupunteScript
    {
        public KetsuWarp(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        private Vehicle _playerVehicle;

        public override void OnStart()
        {
            IdleAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask IdleAsync(CancellationToken ct)
        {
            var time = 0f;
            while (IsActive && !ct.IsCancellationRequested && time < 10)
            {
                time += core.DeltaTime;

                var blip = GTA.World.WaypointBlip;
                if (blip != null && blip.Exists())
                {
                    MoveToAsync(ct).Forget();
                    return;
                }

                await YieldAsync(ct);
            }

            ParupunteEnd();
        }

        protected override void OnFinished()
        {
            if (_playerVehicle != null && _playerVehicle.IsSafeExist())
            {
                _playerVehicle.IsInvincible = false;
            }

            if (core.PlayerPed.IsSafeExist())
            {
                core.PlayerPed.IsInvincible = false;
            }
        }

        private async ValueTask MoveToAsync(CancellationToken ct)
        {
            core.DrawParupunteText("いってらっしゃい！", 3);

            Entity target;
            if (core.PlayerPed.IsInVehicle())
            {
                _playerVehicle = core.PlayerPed.CurrentVehicle;
                target = _playerVehicle;
            }
            else
            {
                target = core.PlayerPed;
            }

            target.IsInvincible = true;

            target.ApplyForce(Vector3.WorldUp * 300.0f);
            GTA.World.AddExplosion(
                target.Position,
                GTA.ExplosionType.Grenade,
                0.5f,
                0.5f,
                core.PlayerPed,
                false);

            if (target is Ped ped)
            {
                ped.Task.Skydive();
            }


            await DelaySecondsAsync(1, ct);


            while (target.IsSafeExist() && !ct.IsCancellationRequested)
            {
                if (target.Model.IsVehicle && !core.PlayerPed.IsInVehicle())
                {
                    if (target.IsSafeExist())
                    {
                        target.IsInvincible = false;
                    }

                    target = core.PlayerPed;
                    target.IsInvincible = true;
                    core.PlayerPed.Task.ClearAllImmediately();
                    core.PlayerPed.Task.Skydive();
                    await DelaySecondsAsync(2, ct);
                }

                var targetBlip = GTA.World.WaypointBlip;
                if (targetBlip == null || !targetBlip.Exists())
                {
                    if (target.IsSafeExist())
                    {
                        target.IsInvincible = false;
                        if (target is Ped ped1)
                        {
                            ped1.ParachuteTo(ped1.Position);
                        }
                    }

                    ParupunteEnd();
                    core.DrawParupunteText("おわり", 3);
                    return;
                }

                if (target is Ped { IsInParachuteFreeFall: false } p2)
                {
                    p2.IsInvincible = false;
                    ParupunteEnd();
                    core.DrawParupunteText("おわり", 3);
                    return;
                }

                var goal = targetBlip.Position;
                var current = target.Position;
                var dir = (goal - current).Normalized;

                var toVector = goal - current;
                var horizontalLength = new Vector3(toVector.X, toVector.Y, 0).Length();


                if (horizontalLength < 30)
                {
                    if (target.IsSafeExist())
                    {
                        target.IsInvincible = false;
                        if (target is Ped ped1)
                        {
                            ped1.ParachuteTo(ped1.Position);
                        }
                    }

                    core.DrawParupunteText("ついたぞ", 3);
                    ParupunteEnd();
                    return;
                }


                if (horizontalLength < 50)
                {
                    target.ApplyForce(dir * 150.0f);
                }
                else
                {
                    target.ApplyForce((dir + Vector3.WorldUp * 0.5f) * 250.0f);
                    GTA.World.AddExplosion(
                        target.Position,
                        GTA.ExplosionType.Grenade,
                        3.5f,
                        0.1f,
                        core.PlayerPed);
                }


                await DelaySecondsAsync(0.2f, ct);
            }

            if (target.IsSafeExist())
            {
                target.IsInvincible = false;
            }

            ParupunteEnd();
        }
    }
}