using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// 緊急脱出
    /// </summary>
    public class EmergencyEscape : InfernoScript
    {
        private EmergencyEscapeConf conf;
        private float EscapeVelocity => conf?.Velocity ?? 25.0f;
        protected override string ConfigFileName { get; } = "EmergencyEscape.conf";

        protected override void Setup()
        {
            conf = LoadConfig<EmergencyEscapeConf>();

            IsActiveRP.Subscribe(x =>
            {
                if (x)
                {
                    ObservePlayerInputAsync(ActivationCancellationToken).Forget();
                }
            });
        }

        private async ValueTask ObservePlayerInputAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsActive)
            {
                if (Game.IsControlPressed(Control.VehicleHorn) && Game.IsControlPressed(Control.VehicleExit))
                {
                    EscapeVehicle();
                }

                await YieldAsync(ct);
            }
        }

        //車に乗ってたら緊急脱出する
        private void EscapeVehicle()
        {
            var player = PlayerPed;
            if (!player.IsInVehicle())
            {
                return;
            }

            var playerVec = player.CurrentVehicle;
            if (!playerVec.IsSafeExist())
            {
                return;
            }

            DelayParachuteAsync(PlayerPed, playerVec, ActivationCancellationToken).Forget();
        }

        private async ValueTask DelayParachuteAsync(Ped player, Vehicle vec, CancellationToken ct)
        {
            try
            {
                Game.Player.CanControlRagdoll = true;
                player.CanRagdoll = true;

                player.Task.LeaveVehicle(vec, LeaveVehicleFlags.WarpOut);
                player.Position += new Vector3(0, 0, 1.0f);
                await DelaySecondsAsync(0.1f, ct);

                var shootPos = player.Position;
                player.SetToRagdoll();
                await YieldAsync(ct);
                
                player.Velocity = new Vector3(0, 0, EscapeVelocity) + vec.Velocity;

                player.IsInvincible = true;
                await DelayAsync(TimeSpan.FromSeconds(1.5f), ct);

                // 発射位置より高い位置にいる場合はパラシュートを開く
                if (player.Position.Z > shootPos.Z)
                {
                    player.ParachuteTo(player.Position);
                }
            }
            finally
            {
                if (PlayerPed.IsSafeExist())
                {
                    PlayerPed.IsInvincible = false;
                }
            }
        }

        #region config

        [Serializable]
        private class EmergencyEscapeConf : InfernoConfig
        {
            private int _velocity = 15;

            public int Velocity
            {
                get => _velocity;
                set => _velocity = value.Clamp(0, 100);
            }

            public override bool Validate()
            {
                return true;
            }
        }

        #endregion

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.EmergencyEscapeTitle;

        public override string Description => PlayerLocalize.EmergencyEscapeDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Player;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            subMenu.AddSlider(
                $"Ejection velocity: {conf.Velocity}",
                PlayerLocalize.EmergencyEscapePower,
                conf.Velocity,
                100,
                x =>
                {
                    x.Value = conf.Velocity;
                    x.Multiplier = 1;
                }, item =>
                {
                    conf.Velocity = item.Value;
                    item.Title = $"Ejection velocity: {conf.Velocity}";
                });


            subMenu.AddButton(InfernoCommon.DefaultValue, "", _ =>
            {
                conf = LoadDefaultConfig<EmergencyEscapeConf>();
                subMenu.Visible = false;
                subMenu.Visible = true;
            });

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(conf);
                DrawText($"Saved to {ConfigFileName}");
            });
        }

        #endregion
    }
}