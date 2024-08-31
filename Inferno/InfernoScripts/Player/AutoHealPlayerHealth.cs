using System;
using System.Threading;
using System.Threading.Tasks;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Player
{
    public class AutoHealPlayerHealth : InfernoScript
    {
        private bool _canHealPlayerHealth = false;
        private readonly int _alertHelthValue = 25;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("autoheal")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("AutoHealPlayerHealth:" + IsActive);

                    if (IsActive)
                    {
                        PlayerHealthHealAsync(ActivationCancellationToken).Forget();
                        PlayerDamageCheckAsync(ActivationCancellationToken).Forget();
                    }
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);
        }

        private async ValueTask PlayerHealthHealAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (PlayerPed.IsSafeExist() && _canHealPlayerHealth)
                {
                    if (PlayerPed.Health < PlayerPed.MaxHealth && PlayerPed.Health >= _alertHelthValue)
                    {
                        var next = Math.Min(PlayerPed.Health + 2, PlayerPed.MaxHealth);
                        PlayerPed.Health = next;
                    }
                    else if (PlayerPed.Armor < 100)
                    {
                        var next = Math.Min(PlayerPed.Armor + 2, 100);
                        PlayerPed.Armor = next;
                    }
                }

                await DelaySecondsAsync(0.5f, ct);
            }
        }

        private async ValueTask PlayerDamageCheckAsync(CancellationToken ct)
        {
            var lastHealth = 0;
            var lastArmor = 0;
            var count = 0;
            while (!ct.IsCancellationRequested)
            {
                if (PlayerPed.IsSafeExist())
                {
                    var currentHealth = PlayerPed.Health;
                    var currentArmor = PlayerPed.Armor;

                    if (currentHealth >= lastHealth && currentArmor >= lastArmor)
                    {
                        if (!_canHealPlayerHealth)
                        {
                            count++;
                            if (count > 5f)
                            {
                                _canHealPlayerHealth = true;
                            }
                        }
                    }
                    else
                    {
                        count = 0;
                        _canHealPlayerHealth = false;
                    }

                    lastHealth = currentHealth;
                    lastArmor = currentArmor;
                }

                await DelaySecondsAsync(1, ct);
            }
        }
    }
}