using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GTA.UI;

namespace Inferno
{
    /// <summary>
    /// 時限式テキスト
    /// </summary>
    internal class TimerUiTextManager
    {
        private readonly InfernoScript parent;
        private readonly Subject<Unit> setTextSubject = new();
        private ReduceCounter reduceCounter;
        private TextElement textElement;

        public TimerUiTextManager(InfernoScript parent)
        {
            this.parent = parent;
        }

        public IObservable<Unit> OnSetTextAsObservable => setTextSubject.AsObservable();

        /// <summary>
        /// 描画テキスト
        /// </summary>
        public TextElement Text
        {
            get
            {
                if (reduceCounter == null || textElement == null)
                {
                    return null;
                }

                return !reduceCounter.IsCompleted ? textElement : null;
            }
        }

        /// <summary>
        /// テキストが有効な状態であるか？
        /// </summary>
        public bool IsEnabled => reduceCounter != null && !reduceCounter.IsCompleted;

        public void Set(TextElement text, float expireSeconds)
        {
            textElement = text;
            reduceCounter = new ReduceCounter((int)(1000 * expireSeconds));
            parent.RegisterCounter(reduceCounter);
            setTextSubject.OnNext(Unit.Default);
        }
    }
}