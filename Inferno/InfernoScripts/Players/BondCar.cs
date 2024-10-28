using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.InfernoScripts.Player
{
    internal class BondCar : InfernoScript
    {
        private float _invincibleMillSeconds = 500;

        protected override void Setup()
        {
            config = LoadConfig<BondCarConfig>();

            CreateInputKeywordAsObservable("BondCar", "bond")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("BondCar:" + IsActive);
                });
            
            IsActiveRP.Subscribe(x =>
            {
                _invincibleMillSeconds = 0;
                if (x)
                {
                    InvincibleVehicleAsync(ActivationCancellationToken).Forget();
                    InputLoopAsync(ActivationCancellationToken).Forget();
                }
            });
            
            CreateTickAsObservable(TimeSpan.FromSeconds(1))
                .Where(_ => IsActive)
                .Select(_ => PlayerPed.IsInVehicle())
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => { PlayerPed.Weapons.Select(WeaponHash.Unarmed, true); });
        }


        private async ValueTask InputLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsActive)
            {
                if (PlayerPed.IsSafeExist() && PlayerPed.IsInVehicle() &&
                    Game.IsControlPressed(Control.VehicleAim) && Game.IsControlPressed(Control.VehicleAttack) &&
                    PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed)
                {
                    Shoot();
                    await DelayAsync(TimeSpan.FromMilliseconds(CoolDownMillSeconds), ct);
                    continue;
                }

                await YieldAsync(ct);
            }
        }


        private void Shoot()
        {
            var v = PlayerPed.CurrentVehicle;
            if (!v.IsSafeExist())
            {
                return;
            }

            //そこら辺の市民のせいにする
            var ped = CachedPeds.Where(x => x.IsSafeExist()).DefaultIfEmpty(PlayerPed).FirstOrDefault();
            _invincibleMillSeconds = 500;
            CreateRpgBullet(v, ped, 1.5f);
            CreateRpgBullet(v, ped, -1.5f);
        }

        private void CreateRpgBullet(Vehicle vehicle, Ped ped, float rightOffset)
        {
            var startPosition = vehicle.GetOffsetFromEntityInWorldCoords(rightOffset, 0, 0.2f);
            var target = vehicle.GetOffsetFromEntityInWorldCoords(0, 1000, 0.2f);

            Function.Call(
                Hash.SHOOT_SINGLE_BULLET_BETWEEN_COORDS,
                startPosition.X,
                startPosition.Y,
                startPosition.Z,
                target.X,
                target.Y,
                target.Z,
                200,
                1,
                Weapon.VEHICLE_ROCKET,
                0,
                1,
                0,
                1000f);
        }


        private async ValueTask InvincibleVehicleAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsActive)
            {
                var v = PlayerPed.CurrentVehicle;

                try
                {
                    if (v.IsSafeExist())
                    {
                        if (_invincibleMillSeconds > 0)
                        {
                            _invincibleMillSeconds -= 100;
                            v.IsExplosionProof = !(_invincibleMillSeconds <= 0);
                        }
                    }

                    await DelaySecondsAsync(0.1f, ct);
                }
                finally
                {
                    if (v.IsSafeExist())
                    {
                        v.IsExplosionProof = false;
                    }
                }
            }
        }

        #region config

        [Serializable]
        private class BondCarConfig : InfernoConfig
        {
            private int _downMillSeconds = 500;

            /// <summary>
            /// ミサイルの発射間隔[ms]
            /// </summary>
            public int DownMillSeconds
            {
                get => _downMillSeconds;
                set => _downMillSeconds = value.Clamp(100, 10000);
            }

            public override bool Validate()
            {
                return DownMillSeconds >= 0;
            }
        }

        protected override string ConfigFileName { get; } = "BondCar.conf";
        private BondCarConfig config;
        private int CoolDownMillSeconds => config?.DownMillSeconds ?? 500;

        #endregion

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.BondCarTitle;

        public override string Description => PlayerLocalize.BondCarDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Player;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            // 戦闘機の数
            subMenu.AddSlider(
                $"Cool time: {config.DownMillSeconds}",
                PlayerLocalize.BondCardInterval,
                config.DownMillSeconds,
                1000 * 10,
                x =>
                {
                    x.Value = config.DownMillSeconds;
                    x.Multiplier = 100;
                }, item =>
                {
                    config.DownMillSeconds = item.Value;
                    item.Title = $"Cool time: {config.DownMillSeconds}";
                });


            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                config = LoadDefaultConfig<BondCarConfig>();
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