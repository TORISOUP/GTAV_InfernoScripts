using System;
using System.Collections.Generic;
using System.Drawing;

using GTA; using UniRx;

namespace Inferno
{
    /// <summary>
    /// 画面左上の表示を管理する
    /// </summary>
    public class ToastTextDrawing : InfernoScript
    {
        private UIContainer _mContainer = null;
        private int _coroutineId = -1;
        //画面表示を消すまでの残りCoroutineループ回数
        private int _currentTickCounter = 0;

        public static ToastTextDrawing Instance { get; private set; }

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            _mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));

            //テキストが設定されていれば一定時間だけ描画
            this.OnDrawingTickAsObservable
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
            if (_coroutineId >= 0)
            {
                //既に実行中のがあれば止める
                StopCoroutine((uint) _coroutineId);
            }
            _coroutineId = (int) StartCoroutine(DrawTextEnumerator(text, time));

        }

        private IEnumerable<Object>  DrawTextEnumerator(string text, float time)
        {
            _mContainer.Items.Clear();
            _currentTickCounter = (int)(time * 10);
            _mContainer.Items.Add(new UIText(text, new Point(0, 0), 0.5f, Color.White, 0, false));

            while (--_currentTickCounter > 0)
            {
                yield return _currentTickCounter;
            }

            _mContainer.Items.Clear();
        }
    }
}
