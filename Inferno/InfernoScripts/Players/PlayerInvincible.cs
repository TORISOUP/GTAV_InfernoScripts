using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;


namespace Inferno.InfernoScripts.Player
{
    public sealed class PlayerInvincible : InfernoScript
    {
        private CancellationTokenSource _lastCts;
        protected override bool DefaultAllOnEnable => false;

        private IDisposable _disposable;

        protected override void Setup()
        {
            config = LoadConfig<PlayerInvincibleConfig>();

            CreateInputKeywordAsObservable("PlayerInvincible", "ainc")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Players Start Invincible:" + IsActive);
                });

            IsActiveRP.Subscribe(x =>
            {
                _disposable?.Dispose();
                if (x)
                {
                    _disposable = OnThinnedTickAsObservable
                        .Where(_ => PlayerPed.IsSafeExist())
                        .Select(_ => (Game.IsMissionActive, PlayerPed.IsAlive))
                        .DistinctUntilChanged()
                        .Skip(1)
                        .Where(s => s.Item1 || s.Item2)
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
                var invincibleSeconds = InvincibleSeconds;
                while (!ct.IsCancellationRequested && ElapsedTime - startTime < invincibleSeconds && IsActive)
                {
                    if (EnableEffect)
                    {
                        var deltaTime = ElapsedTime - startTime;
                        var color = FromHsv(deltaTime * 360f / 0.5f, 1, 1);
                        var rate = ((invincibleSeconds - deltaTime) / invincibleSeconds);
                        NativeFunctions.CreateLight(
                            player.Bones[Bone.SkelHead].Position + player.UpVector * 0.3f,
                            color.R, color.G, color.B, 4 * rate, 100f * rate + 50);
                    }

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

        #region config

        [Serializable]
        private class PlayerInvincibleConfig : InfernoConfig
        {
            private int _invincibleSeconds = 10;
            private bool _enableEffect = true;

            public int InvincibleSeconds
            {
                get => _invincibleSeconds;
                set => _invincibleSeconds = value.Clamp(1, 1000);
            }

            public bool EnableEffect
            {
                get => _enableEffect;
                set => _enableEffect = value;
            }

            public override bool Validate()
            {
                return InvincibleSeconds >= 0;
            }
        }

        protected override string ConfigFileName { get; } = "PlayerInvincible.conf";
        private PlayerInvincibleConfig config;
        private int InvincibleSeconds => config?.InvincibleSeconds ?? 10;
        private bool EnableEffect => config?.EnableEffect ?? true;

        #endregion


        #region UI

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.PlayerInvincibleTitle;
        public override string Description => PlayerLocalize.PlayerInvincibleDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Player;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            subMenu.AddSlider(
                $"Invincible time: {config.InvincibleSeconds}[s]",
                PlayerLocalize.PlayerInvincibleTime,
                config.InvincibleSeconds,
                1000,
                x =>
                {
                    x.Value = config.InvincibleSeconds;
                    x.Multiplier = 1;
                }, item =>
                {
                    config.InvincibleSeconds = item.Value;
                    item.Title = $"Invincible time: {config.InvincibleSeconds}[s]";
                });

            subMenu.AddCheckbox(
                "Enable Effect",
                PlayerLocalize.PlayerInvincibleEffect,
                b => b.Checked = config.EnableEffect,
                b => config.EnableEffect = b);

            subMenu.AddButton(
                "[DEBUG] Add invincibility",
                PlayerLocalize.PlayerInvincibleAdd,
                _ =>
                {
                    if (PlayerPed.IsSafeExist())
                    {
                        PlayerPed.IsInvincible = true;
                        PlayerPed.IsBulletProof = true;
                        PlayerPed.IsFireProof = true;
                        PlayerPed.IsExplosionProof = true;
                        PlayerPed.IsMeleeProof = true;
                        PlayerPed.IsCollisionProof = true;
                        PlayerPed.IsOnlyDamagedByPlayer = true;
                        DrawText("Invincible");
                    }
                });
            
            subMenu.AddButton(
                "[DEBUG] Remove invincibility",
                PlayerLocalize.PlayerInvincibleRemove,
                _ =>
                {
                    if (PlayerPed.IsSafeExist())
                    {
                        PlayerPed.IsInvincible = false;
                        PlayerPed.IsBulletProof = false;
                        PlayerPed.IsFireProof = false;
                        PlayerPed.IsExplosionProof = false;
                        PlayerPed.IsMeleeProof = false;
                        PlayerPed.IsCollisionProof = false;
                        PlayerPed.IsOnlyDamagedByPlayer = false;
                        DrawText("Not Invincible");
                    }
                });


            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                config = LoadDefaultConfig<PlayerInvincibleConfig>();
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(config);
                DrawText($"Saved to {ConfigFileName}");
            });
        }

        #endregion
    }
}