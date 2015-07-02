using System;
using System.Collections;
using System.Collections.Generic;
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
    /// プレイヤーの強制ラグドール状態化(脱力)
    /// </summary>
    public class PlayerRagdoll : InfernoScript
    {
        bool ragdollFlag = false;
        protected override int TickInterval
        {
            get { return 50; }
        }

        protected override void Setup()
        {
            OnTickAsObservable
                    .Where(_ => playerPed.IsSafeExist())
                    .Select(_ => this.IsGamePadPressed(GameKey.Stealth) && this.IsGamePadPressed(GameKey.Jump))
                    .Subscribe(flag =>
                    {
                        var playerChar = Game.Player;

                        if (flag)
                        {
                            SetPlayerRagdoll(playerChar);
                            ragdollFlag = true;
                        }
                        else
                        {
                            if (ragdollFlag)
                            {
                                UnSetPlayerRagdoll(playerChar);
                                ragdollFlag = false;
                            }
                        }
                    });

        }

        void SetPlayerRagdoll(Player PlayerChar)
        {
            var player = PlayerChar.Character;

            player.CanRagdoll = true;
            PlayerChar.CanControlRagdoll = true;          
            PlayerChar.CanControlCharacter = true;
            player.SetToRagdoll(1000000); //時間指定しないとブルブル動く
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
