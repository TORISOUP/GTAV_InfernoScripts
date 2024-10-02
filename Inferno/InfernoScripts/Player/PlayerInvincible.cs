using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Player
{
    public sealed class PlayerInvincible : InfernoScript
    {
        private CancellationTokenSource _lastCts;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("PlayerInvincible", "ainc")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Player Start Invincible:" + IsActive);
                });

            OnThinnedTickAsObservable
                .Where(_ => IsActive && PlayerPed.IsSafeExist())
                .Select(_ => (Game.IsMissionActive, PlayerPed.IsAlive))
                .DistinctUntilChanged()
                .Where(x => x.Item1 || x.Item2)
                .Subscribe(_ =>
                {
                    _lastCts?.Cancel();
                    _lastCts?.Dispose();
                    _lastCts = new CancellationTokenSource();
                    var token = CancellationTokenSource
                        .CreateLinkedTokenSource(_lastCts.Token, ActivationCancellationToken)
                        .Token;
                    PlayerInvincibleAsync(token).Forget();
                });
        }

        private async ValueTask PlayerInvincibleAsync(CancellationToken ct)
        {
            var player = PlayerPed;
            try
            {
                player.IsBulletProof = true;
                player.IsFireProof = true;
                player.IsExplosionProof = true;
                player.IsMeleeProof = true;
                player.IsCollisionProof = true;
                player.IsOnlyDamagedByPlayer = true;

                var startTime = ElapsedTime;
                while (!ct.IsCancellationRequested && ElapsedTime - startTime < 10f)
                {
                    var deltaTime = ElapsedTime - startTime;

                    var color = FromHsv(deltaTime * 360f / 0.5f, 1, 1);

                    var rate = ((10f - deltaTime) / 10f);

                    NativeFunctions.CreateLight(
                        player.Bones[Bone.SkelHead].Position + player.UpVector * 0.3f,
                        color.R, color.G, color.B, 4 * rate, 100f * rate + 50);

                    player.IsInvincible = true;


                    await YieldAsync(ct);
                }
            }
            finally
            {
                player.IsInvincible = false;
                player.IsBulletProof = false;
                player.IsFireProof = false;
                player.IsExplosionProof = false;
                player.IsMeleeProof = false;
                player.IsCollisionProof = false;
                player.IsOnlyDamagedByPlayer = false;
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