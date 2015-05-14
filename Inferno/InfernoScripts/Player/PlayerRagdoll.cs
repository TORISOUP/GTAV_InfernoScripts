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
        protected override int TickInterval
        {
            get { return 50; }
        }

        protected override void Setup()
        {

            OnTickAsObservable
                    .Where(_ => this.GetPlayer().IsSafeExist())
                    .Select(_ => this.IsGamePadPressed(GameKey.Stealth) && this.IsGamePadPressed(GameKey.Jump))
                    .DistinctUntilChanged()
                    .Subscribe(flag =>
                    {
                        var playerChar = Game.Player;
 
                        if (flag)
                        {
                            SetPlayerRagdoll(playerChar);
                        }
                        else
                        {
                            UnSetPlayerRagdoll(playerChar);
                        }
                    });
        }

        void SetPlayerRagdoll(Player PlayerChar)
        {
            var player = PlayerChar.Character;

            player.CanRagdoll = true;
            PlayerChar.CanControlRagdoll = true;          
            player.SetToRagdDoll(0, 0, 0);           
        }

        void UnSetPlayerRagdoll(Player PlayerChar)
        {
            var player = PlayerChar.Character;

            player.CanRagdoll = false;
            PlayerChar.CanControlRagdoll = false;
            //後でラグドール状態にならないのを防ぐために
            player.CanRagdoll = true;
            PlayerChar.CanControlRagdoll = true;
        }

    }
}
