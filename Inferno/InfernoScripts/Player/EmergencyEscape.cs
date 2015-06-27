using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Inferno
{
    /// <summary>
    /// 緊急脱出
    /// </summary>
    public class EmergencyEscape : InfernoScript
    {
        protected override int TickInterval
        {
            get { return 100; }
        }

        protected override void Setup()
        {
            OnTickAsObservable
                .Where(_ => this.IsGamePadPressed(GameKey.VehicleHorn) && this.IsGamePadPressed(GameKey.VehicleExit))
                .Subscribe(_ => EscapeVehicle());
        }

        //車に乗ってたら緊急脱出する
        private void EscapeVehicle()
        {
            var player = playerPed;
            if(!player.IsInVehicle()) return;
            var playerVec = player.CurrentVehicle;
            if(!playerVec.IsSafeExist())return;

            Game.Player.CanControlRagdoll = true;
            player.CanRagdoll = true;

            player.ClearTasksImmediately();
            player.Position += new Vector3(0,0,0.5f);
            player.SetToRagdoll();
            player.ApplyForce(new Vector3(0, 0, 8.0f) + playerVec.Velocity);
            
        }
    }
}
