using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GTA;

namespace Inferno
{
    /// <summary>
    /// 時限式テキスト
    /// </summary>
    internal class TimerUiTextManager
    {
        private readonly InfernoScript parent;
        private ReduceCounter reduceCounter;
        private readonly Subject<Unit> setTextSubject = new();
        private UIText uiText;

        public TimerUiTextManager(InfernoScript parent)
        {
            this.parent = parent;
        }

        public IObservable<Unit> OnSetTextAsObservable => setTextSubject.AsObservable();

        /// <summary>
        /// 描画テキスト
        /// </summary>
        public UIText Text
        {
            get
            {
                if (reduceCounter == null || uiText == null) return null;
                return !reduceCounter.IsCompleted ? uiText : null;
            }
        }

        /// <summary>
        /// テキストが有効な状態であるか？
        /// </summary>
        public bool IsEnabled => reduceCounter != null && !reduceCounter.IsCompleted;

        public void Set(UIText text, float expireSeconds)
        {
            uiText = text;
            reduceCounter = new ReduceCounter((int)(1000 * expireSeconds));
            parent.RegisterCounter(reduceCounter);
            setTextSubject.OnNext(Unit.Default);
        }
    }
}