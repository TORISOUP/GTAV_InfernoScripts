using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("無敵", "おわり")]
    [ParupunteIsono("むてき")]
    internal class PlayerInvincible : ParupunteScript
    {
        public PlayerInvincible(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20000);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
            AddProgressBar(ReduceCounter);
            EffectAsync(ActiveCancellationToken).Forget();
            AttackAsync(ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            var p = core.PlayerPed;
            if (p.IsSafeExist())
            {
                p.IsInvincible = false;
            }
        }

        protected override void OnUpdate()
        {
            var p = core.PlayerPed;
            if (p.IsSafeExist())
            {
                p.IsInvincible = true;
            }
        }

        private async ValueTask AttackAsync(CancellationToken ct)
        {
            var player = core.PlayerPed;

            while (!ct.IsCancellationRequested && IsActive)
            {
                var playerPos = player.Position;

                var pedRange = player.IsInVehicle() ? 6 : 4;
                foreach (var ped in core.CachedPeds.Where(x =>
                             x.IsSafeExist() && x.IsInRangeOf(playerPos, pedRange) && x.IsAlive))
                {
                    ped.SetToRagdoll(10);
                    ped.Task.ClearAllImmediately();
                    ped.Velocity = Vector3.WorldUp * 100f;

                    Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
                    {
                        ped.Position.X,
                        ped.Position.Y,
                        ped.Position.Z,
                        GTA.ExplosionType.Rocket,
                        0.0f,
                        true,
                        false,
                        0.1f
                    });

                    await YieldAsync(ct);
                }

                var veRange = player.IsInVehicle() ? 10 : 8;

                foreach (var ve in core.CachedVehicles.Where(x =>
                             x.IsSafeExist() && x.IsInRangeOf(playerPos, veRange) && x != player.CurrentVehicle && x.IsAlive))
                {
                    ve.Velocity = Vector3.WorldUp * 100f;

                    Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
                    {
                        ve.Position.X,
                        ve.Position.Y,
                        ve.Position.Z,
                        GTA.ExplosionType.Rocket,
                        0.0f,
                        true,
                        false,
                        0.1f
                    });
                    await YieldAsync(ct);

                }

                await YieldAsync(ct);
            }
        }


        private async ValueTask EffectAsync(CancellationToken ct)
        {
            var player = core.PlayerPed;


            var startTime = core.ElapsedTime;
            while (!ct.IsCancellationRequested && IsActive)
            {
                var deltaTime = core.ElapsedTime - startTime;
                var color = FromHsv(deltaTime * 360f / 0.5f, 1, 1);
                NativeFunctions.CreateLight(
                    player.Bones[Bone.SkelHead].Position + player.UpVector * 0.3f,
                    color.R, color.G, color.B, 4, 150);

                player.IsInvincible = true;

                await YieldAsync(ct);
            }
        }

        private static Color FromHsv(float h, float s, float v)
        {
            var hi = Convert.ToInt32(Math.Floor(h / 60)) % 6;
            var f = h / 60 - (float)Math.Floor(h / 60);

            v = v * 255;
            var vInt = Convert.ToInt32(v);
            var pInt = Convert.ToInt32(v * (1 - s));
            var qInt = Convert.ToInt32(v * (1 - f * s));
            var tInt = Convert.ToInt32(v * (1 - (1 - f) * s));

            if (hi == 0)
            {
                return Color.FromArgb(255, vInt, tInt, pInt);
            }

            if (hi == 1)
            {
                return Color.FromArgb(255, qInt, vInt, pInt);
            }

            if (hi == 2)
            {
                return Color.FromArgb(255, pInt, vInt, tInt);
            }

            if (hi == 3)
            {
                return Color.FromArgb(255, pInt, qInt, vInt);
            }

            if (hi == 4)
            {
                return Color.FromArgb(255, tInt, pInt, vInt);
            }

            return Color.FromArgb(255, vInt, pInt, qInt);
        }
    }
}