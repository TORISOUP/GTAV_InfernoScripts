using System;
using System.Reactive.Linq;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Player
{
    //ふわふわジャンプ
    internal class Floating : InfernoScript
    {
        private float _currentPower;

        protected override void Setup()
        {
            OnTickAsObservable
                .Where(_ => Game.IsControlPressed(Control.Jump) && Game.IsControlPressed(Control.Sprint))
                .Subscribe(_ =>
                {
                    _currentPower = (PlayerPed.IsFloating(0.25f) || PlayerPed.IsInAir)
                        ? Math.Max(0.3f, _currentPower * 0.8f)
                        : 1.3f;
                    PlayerPed.ApplyForce(Vector3.WorldUp * _currentPower);
                });
        }
    }
}