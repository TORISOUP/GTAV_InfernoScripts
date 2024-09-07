using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA.UI;
using Inferno.Utilities;

namespace Inferno
{
    /// <summary>
    /// 画面左上の表示を管理する
    /// </summary>
    public sealed class ToastTextDrawing : InfernoScript
    {
        private ContainerElement _container;

        public static ToastTextDrawing Instance { get; private set; }
        private CancellationTokenSource _cts;

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            _container = new ContainerElement(new Point(0, 0), new Size(500, 20));

            //テキストが設定されていれば一定時間だけ描画
            OnDrawingTickAsObservable
                .Where(_ => _container.Items.Count > 0)
                .Subscribe(_ => _container.Draw());

            OnAllOnCommandObservable
                .Subscribe(_ => DrawText("Inferno:AllOn"));

            OnAbortAsync.Subscribe(_ =>
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                _container.Items.Clear();
            });
        }

        /// <summary>
        /// 画面左上に表示する
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="time">時間</param>
        public void DrawDebugText(string text, float time)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _container.Items.Clear();

            DrawTextAsync(text, time, _cts.Token).Forget();
        }

        private async ValueTask DrawTextAsync(string text, float time, CancellationToken ct)
        {
            _container.Items.Add(new TextElement(
                text,
                new Point(0, 0),
                0.5f,
                Color.White,
                0,
                Alignment.Left,
                false,
                true));


            await DelaySecondsAsync(time, ct);
            _container.Items.Clear();
        }
    }
}