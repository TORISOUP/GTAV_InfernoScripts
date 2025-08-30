using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
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

            Game.Player.CanControlRagdoll = true;
            DelayParachuteAsync(PlayerPed, playerVec, ActivationCancellationToken).Forget();

            {
                var ped = playerVec.GetPedOnSeat(VehicleSeat.Passenger);
                if (ped.IsSafeExist()) DelayParachuteAsync(ped, playerVec, ActivationCancellationToken).Forget();
            }
            {
                var ped = playerVec.GetPedOnSeat(VehicleSeat.LeftRear);
                if (ped.IsSafeExist()) DelayParachuteAsync(ped, playerVec, ActivationCancellationToken).Forget();
            }
            {
                var ped = playerVec.GetPedOnSeat(VehicleSeat.RightRear);
                if (ped.IsSafeExist()) DelayParachuteAsync(ped, playerVec, ActivationCancellationToken).Forget();
            }
        }

        private async ValueTask DelayParachuteAsync(Ped ped, Vehicle vec, CancellationToken ct)
        {
            if (!ped.IsSafeExist()) return;
            if (!vec.IsSafeExist()) return;
            var isRequirePersitent = false;

            try
            {
                if (ped == PlayerPed)
                {
                    isRequirePersitent = vec.IsPersistent;
                    vec.IsPersistent = true;
                }
                else
                {
                    ped.SetNotChaosPed(true);
                }

                ped.CanRagdoll = true;

                Function.Call(Hash.SET_ENTITY_COLLISION, ped.Handle, false, true);

                var vecVelocity = vec.Velocity;
                ped.Task.LeaveVehicle(vec, LeaveVehicleFlags.WarpOut);

                ped.PositionNoOffset = ped.Position + vec.UpVector * 0.5f + Vector3.RandomXY();
                if (!ped.IsSafeExist()) return;

                var shootPos = ped.Position;
                ped.SetToRagdoll(100);
                ped.Velocity = vec.UpVector * EscapeVelocity + vecVelocity;

                ped.IsInvincible = true;

                await DelaySecondsAsync(0.5f, ct);
                if (!ped.IsSafeExist()) return;
                Function.Call(Hash.SET_ENTITY_COLLISION, ped.Handle, true, true);

                await DelayAsync(TimeSpan.FromSeconds(1.0f), ct);
                if (!ped.IsSafeExist()) return;

                // 発射位置より高い位置にいる場合はパラシュートを開く
                if (ped.Position.Z > shootPos.Z)
                {
                    ped.ParachuteTo(ped.Position);
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    ped.IsInvincible = false;
                    Function.Call(Hash.SET_ENTITY_COLLISION, ped.Handle, true, true);
                }

                if (ped == PlayerPed)
                {
                    vec.IsPersistent = isRequirePersitent;
                }
                else
                {
                    ped.SetNotChaosPed(false);
                }
            }
        }

        #region config

        [Serializable]
        private class EmergencyEscapeConf : InfernoConfig
        {
            private int _velocity = 35;

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