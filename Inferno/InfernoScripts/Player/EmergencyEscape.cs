using System.Collections.Generic;
using GTA;
using GTA.Math;
using Inferno.Utilities;
using UniRx;

namespace Inferno
{
    /// <summary>
    /// 緊急脱出
    /// </summary>
    public class EmergencyEscape : InfernoScript
    {
        #region config

        class EmergencyEscapeConf : InfernoConfig
        {
            public float EscapePower { get; set; } = 60.0f;
            public float OpenParachutoSeconds { get; set; } = 1.5f;
            public override bool Validate()
            {
                return true;
            }
        }

        #endregion

        private EmergencyEscapeConf conf;
        private float EscapePower => conf?.EscapePower ?? 60.0f;
        private float OpenParachutoSeconds => conf?.OpenParachutoSeconds ?? 1.5f;
        protected override string ConfigFileName { get; } = "EmergencyEscape.conf";

        protected override void Setup()
        {
            conf = LoadConfig<EmergencyEscapeConf>();

            OnTickAsObservable
                .Where(_ => this.IsGamePadPressed(GameKey.VehicleHorn) && this.IsGamePadPressed(GameKey.VehicleExit))
                .Subscribe(_ => EscapeVehicle());
        }

        //車に乗ってたら緊急脱出する
        private void EscapeVehicle()
        {
            var player = PlayerPed;
            if (!player.IsInVehicle()) return;
            var playerVec = player.CurrentVehicle;
            if (!playerVec.IsSafeExist()) return;

            Game.Player.CanControlRagdoll = true;
            player.CanRagdoll = true;

            player.ClearTasksImmediately();
            player.Position += new Vector3(0, 0, 0.5f);
            player.SetToRagdoll();
            player.ApplyForce(new Vector3(0, 0, EscapePower) + playerVec.Velocity, InfernoUtilities.CreateRandomVector() * 10.0f);

            StartCoroutine(DelayParachute());
        }

        private IEnumerable<object> DelayParachute()
        {
            PlayerPed.IsInvincible = true;
            yield return WaitForSeconds(OpenParachutoSeconds);
            PlayerPed.IsInvincible = false;
            PlayerPed.ParachuteTo(PlayerPed.Position);
        }
    }
}
