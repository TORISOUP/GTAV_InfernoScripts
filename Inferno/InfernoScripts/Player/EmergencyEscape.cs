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
        protected override void Setup()
        {
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
            player.ApplyForce(new Vector3(0, 0, 40.0f) + playerVec.Velocity, InfernoUtilities.CreateRandomVector() * 10.0f);

            StartCoroutine(DelayParachute());
        }

        private IEnumerable<object> DelayParachute()
        {
            PlayerPed.IsInvincible = true;
            yield return WaitForSeconds(1.5f);
            PlayerPed.IsInvincible = false;
            PlayerPed.ParachuteTo(PlayerPed.Position);
        }
    }
}
