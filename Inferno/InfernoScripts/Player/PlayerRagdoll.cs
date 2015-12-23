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
        protected override int TickInterval
        {
            get { return 50; }
        }

        protected override void Setup()
        {
            OnTickAsObservable
                    .Where(_ => PlayerPed.IsSafeExist())
                    .Select(_ => this.IsGamePadPressed(GameKey.Stealth) && this.IsGamePadPressed(GameKey.Jump))
                    .Subscribe(flag =>
                    {
                        var playerChar = Game.Player;

                        if (flag)
                        {
                            SetPlayerRagdoll(playerChar);
                        }
                    });

        }

        void SetPlayerRagdoll(Player PlayerChar)
        {
            var player = PlayerChar.Character;
            player.SetToRagdoll(); //時間指定しなくても大丈夫っぽい
        }
    }
}
