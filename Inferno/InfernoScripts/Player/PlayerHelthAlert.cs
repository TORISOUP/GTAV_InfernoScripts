using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using Inferno;

namespace Inferno
{
    /// <summary>
    /// 体力が減ったら警告する
    /// </summary>
    internal class PlayerHelthAlert : InfernoScript
    {
        float omega = 0.4f;
        float time = 0.0f;
        int amplitude = 25;
        int offset = 15;

        private UIContainer _mContainer;
        private int ScreenHeight;
        private int ScreenWidth;
        private int alertHelthValue = 25;

        protected override int TickInterval => 50;

        protected override void Setup()
        {
            var screenResolution = NativeFunctions.GetScreenResolution();
            ScreenHeight = (int) screenResolution.Y;
            ScreenWidth = (int) screenResolution.X;

            _mContainer = new UIContainer(
                new Point(0, 0), new Size(ScreenWidth, ScreenHeight));

            OnDrawingTickAsObservable
                .Subscribe(_ => _mContainer.Draw());

            OnTickAsObservable
                .Where(_ => playerPed.IsSafeExist() && playerPed.Health < alertHelthValue && playerPed.IsAlive)
                .Subscribe(_ =>
                {
                    //体力が減ったら画面を点滅させる
                    time += 1;
                    var alpha = offset + amplitude*Math.Abs(Math.Sin(omega*time));

                    _mContainer.Items.Clear();
                    var rect = new UIRectangle(new Point(0, 0), new Size(ScreenWidth, ScreenHeight),
                        Color.FromArgb((int) alpha, 255, 0, 0));
                    _mContainer.Items.Add(rect);
                });

            OnTickAsObservable
                .Select(_ => playerPed.IsSafeExist() && playerPed.Health < alertHelthValue)
                .DistinctUntilChanged()
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    time = 0;
                    _mContainer.Items.Clear();
                });

        }
    }
}
