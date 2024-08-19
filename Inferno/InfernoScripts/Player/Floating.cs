using System;
using GTA.Math;
using System.Reactive.Linq;


namespace Inferno.InfernoScripts.Player
{
    //ふわふわジャンプ
    class Floating : InfernoScript
    {
        protected override void Setup()
        {
            this.CreateTickAsObservable(TimeSpan.FromMilliseconds(50))
                .Where(_ => this.IsGamePadPressed(GameKey.Sprint) && this.IsGamePadPressed(GameKey.Jump))
                .Subscribe(_ =>
                {
                    PlayerPed.ApplyForce(Vector3.WorldUp * 1.1f);
                });
        }
    }
}
