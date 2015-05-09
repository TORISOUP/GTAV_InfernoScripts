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
                .Subscribe(_ => 
                    {
                        var player = this.GetPlayer();
                        var PlayerChar = Game.Player;
                        if (!player.IsSafeExist()) { return; } 

                        if (this.IsGamePadPressed(GameKey.Stealth) && this.IsGamePadPressed(GameKey.Jump))
                        {
                            SetPlayerRagdoll(player, PlayerChar);  
                        }
                        else if (RagdollFlag) 
                        {
                            UnSetPlayerRagdoll(player, PlayerChar); 
                        }
                    });


        }


        void SetPlayerRagdoll(Ped player, Player PlayerChar)
        {
            RagdollFlag = true;
            player.CanRagdoll = true;
            PlayerChar.CanControlRagdoll = true;          
            player.SetToRagdDoll(0, 0, 0);           
        }

        void UnSetPlayerRagdoll(Ped player, Player PlayerChar)
        { 
            RagdollFlag = false;
            player.CanRagdoll = false;
            PlayerChar.CanControlRagdoll = false;
        }

    }
}
