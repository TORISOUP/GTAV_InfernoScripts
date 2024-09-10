using System;
using System.Reactive.Linq;
using GTA;

namespace Inferno
{
    /// <summary>
    /// プレイヤーの強制ラグドール状態化(脱力)
    /// </summary>
    public class PlayerRagdoll : InfernoScript
    {
        protected override void Setup()
        {
            CreateTickAsObservable(TimeSpan.FromMilliseconds(50))
                .Where(_ => Game.IsControlPressed(Control.Duck) && Game.IsControlPressed(Control.Jump))
                .Subscribe(_ =>
                {
                    var playerChar = Game.Player;
                    SetPlayerRagdoll(playerChar);
                });
        }

        private void SetPlayerRagdoll(Player PlayerChar)
        {
            var player = PlayerChar.Character;
            player.SetToRagdoll(); //時間指定しなくても大丈夫っぽい
        }
    }
}