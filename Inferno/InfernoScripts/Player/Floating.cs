using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA.Math;
using UniRx;

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
