using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
 
namespace Inferno
{
    /// <summary>
    /// プレイヤーの強制ラグドール状態化
    /// </summary>
    public class PlayerRagdoll : InfernoScript
    {
        bool RagdollFlag = false;

        protected override int TickInterval
        {
            get { return 50; }
        }

        protected override void Setup()
        {

            OnTickAsObservable
                .Where(
                    _ =>
                        this.IsGamePadPressed(GameKey.Stealth) && this.IsGamePadPressed(GameKey.Jump))
                .Subscribe(_ => SetPlayerRagdoll());
            OnTickAsObservable
                .Where(
                    _ =>
                        !this.IsGamePadPressed(GameKey.Stealth) || !this.IsGamePadPressed(GameKey.Jump))
                .Subscribe(_ => UnSetPlayerRagdoll());

        }


        void SetPlayerRagdoll()
        {
            var player = this.GetPlayer();
            var playerChar = NativeFunctions.GetPlayerId();
            if (!player.IsSafeExist()) { return;}

            RagdollFlag = true;
            player.CanRagdoll = true;
            playerChar.CanControlRagdoll = true;
            
            if (player.IsRagdoll) { return; }
            player.SetPedToRagdoll(0, 0, 0);
           
        }

        void UnSetPlayerRagdoll()
        {
            var player = this.GetPlayer();
            var playerChar = NativeFunctions.GetPlayerId();
            if (!player.IsSafeExist() && !RagdollFlag) { return; }
            RagdollFlag = false;
            player.CanRagdoll = false;
            playerChar.CanControlRagdoll = false;
            
        }

    }
}
