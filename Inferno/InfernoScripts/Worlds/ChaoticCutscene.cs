using System.Threading;
using System.Threading.Tasks;
using Inferno.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    public sealed class ChaoticCutscene : InfernoScript
    {
        [Serializable]
        private class ChaoticCutsceneConfig : InfernoConfig
        {
            private int _frequency = 3;
            private int _probability = 5;

            public int Frequency
            {
                get => _frequency;
                set => _frequency = value.Clamp(1, 30);
            }

            public int Probability
            {
                get => _probability;
                set => _probability = value.Clamp(1, 100);
            }

            public override bool Validate()
            {
                if (_frequency <= 0) return false;
                if (_probability < 1) return false;
                if (_probability > 100) return false;
                return true;
            }
        }


        private readonly HashSet<Ped> _targets = new();
        protected override string ConfigFileName { get; } = "ChaoticCutscene.conf";

        private ChaoticCutsceneConfig _config;

        protected override bool DefaultAllOnEnable => false;

        private bool IsInCutScene => Game.IsCutsceneActive && !Game.Player.CanControlCharacter;

        protected override void Setup()
        {
            _config ??= LoadConfig<ChaoticCutsceneConfig>();

            CreateInputKeywordAsObservable("ChaoticCutscene", "ccutscene")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText($"ChaoticCutscene:{IsActive}");
                });

            IsActiveRP
                .Subscribe(x =>
                {
                    _targets.Clear();

                    if (x)
                    {
                        LoopAsync(ActivationCancellationToken).Forget();
                    }
                });
        }

        private async ValueTask LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                while (!IsInCutScene)
                {
                    await DelaySecondsAsync(1, ct);
                }

                await CutSceneAsync(ct);
            }
        }

        private async ValueTask CutSceneAsync(CancellationToken ct)
        {
            _targets.Clear();

            while (IsInCutScene && !ct.IsCancellationRequested)
            {
                var pedRange = 20;
                var playerPos = PlayerPed.Position;

                CutScenePedAsync(PlayerPed, ct).Forget();

                foreach (var ped in CachedPeds.Where(x =>
                             x.IsSafeExist() && x.IsInRangeOf(playerPos, pedRange) &&
                             x.IsAlive))
                {
                    CutScenePedAsync(ped, ct).Forget();
                }

                await DelaySecondsAsync(2, ct);
            }
        }

        private async ValueTask CutScenePedAsync(Ped ped, CancellationToken ct)
        {
            if (!_targets.Add(ped)) return;
            if (!ped.IsSafeExist()) return;

            var lastInvincible = !ped.IsPlayer && ped.IsInvincible;
            try
            {
                while (ped.IsSafeExist() && IsInCutScene && !ct.IsCancellationRequested)
                {
                    // 抽選
                    while (ped.IsSafeExist() && IsInCutScene && !ct.IsCancellationRequested)
                    {
                        if (Random.Next(0, 100) < _config.Probability)
                        {
                            break;
                        }

                        await DelaySecondsAsync(_config.Frequency, ct);
                    }


                    var randomSeconds = Random.Next(5, 10);
                    var shuffle = Random.Next(0, 7);
                    switch (shuffle)
                    {
                        case 0:
                            break;

                        case 1:
                            RotationAsync(ped, randomSeconds, ct).Forget();
                            break;

                        case 2:
                            SlideMoveAsync(ped, randomSeconds, ct).Forget();
                            break;

                        case 3:
                            RagdollAsync(ped, randomSeconds, ct).Forget();
                            RotationAsync(ped, randomSeconds, ct).Forget();
                            break;

                        case 4:
                            RagdollAsync(ped, randomSeconds, ct).Forget();
                            SlideMoveAsync(ped, randomSeconds, ct).Forget();
                            break;

                        case 5:
                            RotationAsync(ped, randomSeconds, ct).Forget();
                            SlideMoveAsync(ped, randomSeconds, ct).Forget();
                            break;

                        case 6:
                            ShakeMoveAsync(ped, randomSeconds, ct).Forget();
                            RagdollAsync(ped, randomSeconds, ct).Forget();
                            break;

                        default:
                            RotationAsync(ped, randomSeconds, ct).Forget();
                            SlideMoveAsync(ped, randomSeconds, ct).Forget();
                            RagdollAsync(ped, randomSeconds, ct).Forget();
                            break;
                    }

                    ped.IsInvincible = true;

                    await DelaySecondsAsync(randomSeconds, ct);
                }
            }
            finally
            {
                ped.IsInvincible = lastInvincible;
                _targets.Remove(ped);
            }
        }

        // 脱力状態
        private async ValueTask RagdollAsync(Ped ped, float seconds, CancellationToken ct)
        {
            var elapsedTime = 0f;
            while (ped.IsSafeExist() && IsInCutScene && !ct.IsCancellationRequested)
            {
                elapsedTime += DeltaTime;
                if (elapsedTime >= seconds) return;

                ped.Task.ClearAll();
                ped.SetToRagdoll((int)(DeltaTime * 1000) + 16);
                await YieldAsync(ct);
            }
        }

        private async ValueTask RotationAsync(Ped ped, float seconds, CancellationToken ct)
        {
            var elapsedTime = 0f;

            var randomXYZ = Vector3.RandomXYZ();
            var power = (float)Random.NextDouble() * 100f;

            while (ped.IsSafeExist() && IsInCutScene && !ct.IsCancellationRequested)
            {
                elapsedTime += DeltaTime;
                if (elapsedTime >= seconds) return;

                ped.RotationVelocity = (power * randomXYZ);
                await YieldAsync(ct);
            }
        }

        private async ValueTask SlideMoveAsync(Ped ped, float seconds, CancellationToken ct)
        {
            var elapsedTime = 0f;

            var randomXYZ = Vector3.RandomXYZ();
            var power = (float)Random.NextDouble();

            while (ped.IsSafeExist() && IsInCutScene && !ct.IsCancellationRequested)
            {
                elapsedTime += DeltaTime;
                if (elapsedTime >= seconds) return;

                ped.Velocity = (power * randomXYZ);
                await YieldAsync(ct);
            }
        }

        private async ValueTask ShakeMoveAsync(Ped ped, float seconds, CancellationToken ct)
        {
            var elapsedTime = 0f;

            while (ped.IsSafeExist() && IsInCutScene && !ct.IsCancellationRequested)
            {
                elapsedTime += DeltaTime;
                if (elapsedTime >= seconds) return;
                var randomXYZ = Vector3.RandomXYZ();
                var power = (float)Random.NextDouble() * 10f;
                ped.Velocity = (power * randomXYZ);
                await YieldAsync(ct);
            }
        }

        public override bool UseUI => true;
        public override string DisplayName => MiscLocalization.CutSceneTitle;

        public override string Description => MiscLocalization.CutSceneDescription;

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Misc;


        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            _config ??= LoadConfig<ChaoticCutsceneConfig>();


            subMenu.AddSlider(
                $"Frequency: {_config.Frequency}[s]",
                MiscLocalization.CutSceneFrequecy,
                _config.Frequency,
                30,
                x =>
                {
                    x.Value = _config.Frequency;
                    x.Multiplier = 1;
                }, item =>
                {
                    _config.Frequency = item.Value;
                    item.Title = $"Frequency: {_config.Frequency}[s]";
                });

            subMenu.AddSlider(
                $"Probability: {_config.Probability}[%]",
                MiscLocalization.CutSceneProbabillity,
                _config.Probability,
                100,
                x =>
                {
                    x.Value = _config.Probability;
                    x.Multiplier = 1;
                }, item =>
                {
                    _config.Probability = item.Value;
                    item.Title = $"Probability: {_config.Probability}[%]";
                });

            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                _config = LoadDefaultConfig<ChaoticCutsceneConfig>();
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(_config);
                DrawText($"Saved to {ConfigFileName}");
            });
        }
    }
}