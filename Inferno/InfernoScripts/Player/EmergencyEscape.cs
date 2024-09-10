using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno
{
    /// <summary>
    /// 緊急脱出
    /// </summary>
    public class EmergencyEscape : InfernoScript
    {
        private EmergencyEscapeConf conf;
        private float EscapePower => conf?.EscapePower ?? 60.0f;
        protected override string ConfigFileName { get; } = "EmergencyEscape.conf";

        protected override void Setup()
        {
            conf = LoadConfig<EmergencyEscapeConf>();

            OnThinnedTickAsObservable
                .Where(_ => Game.IsControlPressed(Control.VehicleHorn) && Game.IsControlPressed(Control.VehicleExit))
                .Subscribe(_ => EscapeVehicle());
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

            DelayParachuteAsync(PlayerPed, playerVec, DestroyCancellationToken).Forget();
        }

        private async ValueTask DelayParachuteAsync(Ped player, Vehicle vec, CancellationToken ct)
        {
            try
            {
                Game.Player.CanControlRagdoll = true;
                player.CanRagdoll = true;

                player.Task.LeaveVehicle(vec, LeaveVehicleFlags.WarpOut);
                await YieldAsync(ct);
                player.Position += new Vector3(0, 0, 1.0f);
                var shootPos = player.Position;
                player.SetToRagdoll();
                player.ApplyForce(new Vector3(0, 0, EscapePower) + vec.Velocity,
                    Vector3.Zero, ForceType.MaxForceRot);

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

        private class EmergencyEscapeConf : InfernoConfig
        {
            public float EscapePower { get; } = 60.0f;

            public override bool Validate()
            {
                return true;
            }
        }

        #endregion
    }
}