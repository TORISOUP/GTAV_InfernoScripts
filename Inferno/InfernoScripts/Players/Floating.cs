using System;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;

namespace Inferno.InfernoScripts.Player
{
    //ふわふわジャンプ
    internal class Floating : InfernoScript
    {
        private float _currentPower;

        private IDisposable _disposable;

        protected override void Setup()
        {
            IsActiveRP.Subscribe(x =>
            {
                _disposable?.Dispose();
                if (x)
                {
                    _disposable = OnTickAsObservable
                        .Where(_ => Game.IsControlPressed(Control.Jump) && Game.IsControlPressed(Control.Sprint))
                        .Subscribe(_ =>
                        {
                            _currentPower = (PlayerPed.IsFloating(0.25f) || PlayerPed.IsInAir)
                                ? Math.Max(0.3f, _currentPower * 0.8f)
                                : 1.3f;
                            PlayerPed.ApplyForce(Vector3.WorldUp * _currentPower);
                        });
                }
            });
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.FloatingTitle;

        public override string Description => PlayerLocalize.FloatingDescription;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Player;

        #endregion
    }
}