using System;
using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;



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
                    .Where(_ => this.IsGamePadPressed(GameKey.Stealth) && this.IsGamePadPressed(GameKey.Jump))
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
