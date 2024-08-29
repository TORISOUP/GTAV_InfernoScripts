using System;
using System.Reactive.Linq;
using GTA.Math;

namespace Inferno.InfernoScripts.Player
{
    //ふわふわジャンプ
    internal class Floating : InfernoScript
    {
        protected override void Setup()
        {
            CreateTickAsObservable(TimeSpan.FromMilliseconds(50))
                .Where(_ => this.IsGamePadPressed(GameKey.Sprint) && this.IsGamePadPressed(GameKey.Jump))
                .Subscribe(_ => { PlayerPed.ApplyForce(Vector3.WorldUp * 1.3f); });
        }
    }
}