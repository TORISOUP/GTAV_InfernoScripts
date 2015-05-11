using System;
using System.Collections;
using System.Windows;
using GTA;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Inferno
{
    /// <summary>
    /// 画面左上の表示を管理する
    /// </summary>
    public class ToastTextDrawing : InfernoScript
    {
        private UIContainer _mContainer = null;
        private int coroutineId = -1;
        //画面表示を消すまでの残りCoroutineループ回数
        private int currentTickCounter = 0;

        public static ToastTextDrawing Instance { get; private set; }

        protected override int TickInterval
        {
            get { return 0; }
        }

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            _mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));

            //テキストが設定されていれば一定時間だけ描画
            this.OnTickAsObservable
                .Where(_ => _mContainer.Items.Count > 0)
                .Subscribe(_ => _mContainer.Draw());

            this.OnAllOnCommandObservable
                .Subscribe(_ => DrawText("Inferno:AllOn", 3.0f));
        }

        /// <summary>
        /// 画面左上に表示する
        /// </summary>
        /// <param name="text">文字列</param>
        /// <param name="time">時間</param>
        public void DrawDebugText(string text, float time)
        {
            if (coroutineId >= 0)
            {
                //既に実行中のがあれば止める
                StopCoroutine((uint) coroutineId);
            }
            coroutineId = (int) StartCoroutine(DrawTextEnumerator(text, time));

        }

        private IEnumerator DrawTextEnumerator(string text, float time)
        {
            _mContainer.Items.Clear();
            currentTickCounter = (int)(time * 10);
            _mContainer.Items.Add(new UIText(text, new Point(0, 0), 0.5f, Color.White, 0, false));

            while (--currentTickCounter > 0)
            {
                yield return currentTickCounter;
            }

            _mContainer.Items.Clear();
        }
    }
}
