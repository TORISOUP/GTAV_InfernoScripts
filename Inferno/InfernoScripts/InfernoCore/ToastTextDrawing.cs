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
        private UIContainer mContainer = null;
        private Subject<Unit> onDrawStart = new Subject<Unit>();
        private Subject<Unit> onDrawEnd = new Subject<Unit>();
        private int coroutineId = -1;

        public static ToastTextDrawing Instance { get; private set; }

        protected override int TickInterval
        {
            get { return 0; }
        }

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));

            //テキストが設定されていれば一定時間だけ描画
            this.OnTickAsObservable
                .SkipUntil(onDrawStart)
                .TakeUntil(onDrawEnd)
                .Repeat()
                .Subscribe(_ => mContainer.Draw());

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
            StartCoroutine(drawTextEnumerator(text, time));
        }

        private IEnumerator drawTextEnumerator(string text, float time)
        {
            mContainer.Items.Clear();
            Interval = 0;
            mContainer.Items.Add(new UIText(text, new Point(0, 0), 0.5f, Color.White, 0, false));
            onDrawStart.OnNext(Unit.Default);
            foreach (var s in WaitForSecond(time))
            {
                yield return s;
            }
            onDrawEnd.OnNext(Unit.Default);
            mContainer.Items.Clear();
            Interval = 10000;//表示していない間は遅くする
        }
    }
}
