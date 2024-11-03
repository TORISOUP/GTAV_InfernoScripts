using System;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GTA;
using GTA.UI;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;

namespace Inferno
{
    /// <summary>
    /// 体力が減ったら警告する
    /// </summary>
    internal class PlayerHealthAlert : InfernoScript
    {
        private readonly int alertHelthValue = 125;
        private readonly int amplitude = 25;
        private readonly int offset = 15;
        private readonly float omega = 0.4f;
        private ContainerElement _mContainer;
        private int ScreenHeight;
        private int ScreenWidth;
        private float time;
        private CompositeDisposable _compositeDisposable;
        private UIRectangle _rect;

        protected override void Setup()
        {
            var screenResolution = NativeFunctions.GetScreenResolution();
            ScreenHeight = (int)screenResolution.Y;
            ScreenWidth = (int)screenResolution.X;

            _mContainer = new ContainerElement(
                new Point(0, 0), new Size(ScreenWidth, ScreenHeight));

            IsActiveRP.Subscribe(x =>
            {
                
                _rect = null;
                _mContainer.Items.Clear();
                _compositeDisposable?.Dispose();
                
                if (x)
                {
                    _compositeDisposable = new CompositeDisposable();

                    OnDrawingTickAsObservable
                        .Subscribe(_ => _mContainer.Draw())
                        .AddTo(_compositeDisposable);

                    var checkObservable = CreateTickAsObservable(TimeSpan.FromMilliseconds(50));

                    checkObservable
                        .Where(_ => PlayerPed.IsSafeExist() && PlayerPed.Health < alertHelthValue && PlayerPed.IsAlive)
                        .Subscribe(_ =>
                        {
                  
                            //体力が減ったら画面を点滅させる
                            time += 1;
                            var alpha = offset + amplitude * Math.Abs(Math.Sin(omega * time));

                            if (_rect == null)
                            {
                                _rect = new UIRectangle(new Point(0, 0), new Size(ScreenWidth, ScreenHeight),
                                    Color.FromArgb((int)alpha, 255, 0, 0));
                                _mContainer.Items.Add(_rect);
                            }
                            else
                            {
                                _rect.Color = Color.FromArgb((int)alpha, 255, 0, 0);
                            }
                       
                        }).AddTo(_compositeDisposable);

                    checkObservable
                        .Select(_ => PlayerPed.IsSafeExist() && PlayerPed.Health < alertHelthValue)
                        .DistinctUntilChanged()
                        .Where(s => !s)
                        .Subscribe(_ =>
                        {
                            time = 0;
                            _rect=null;
                            _mContainer.Items.Clear();
                        }).AddTo(_compositeDisposable);
                }
            });
        }


        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.AlertHealthTitle;

        public override string Description => PlayerLocalize.AlertHealthDescription;
        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Player;
    }
}