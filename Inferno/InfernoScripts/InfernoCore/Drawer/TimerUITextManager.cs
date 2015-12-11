using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace Inferno
{
    /// <summary>
    ///　時限式テキスト
    /// </summary>
    class TimerUiTextManager
    {
        private InfernoScript parent;
        private ReduceCounter reduceCounter;
        private UIText uiText;
        private Subject<Unit> setTextSubject = new Subject<Unit>();
        private Subject<Unit> textExpiredSubject = new Subject<Unit>(); 
        public IObservable<Unit> OnSetTextAsObservable => setTextSubject.AsObservable();
        public IObservable<Unit> OnTextExpiredObservable => textExpiredSubject.AsObservable(); 
        public TimerUiTextManager(InfernoScript parent)
        {
            this.parent = parent;
        }

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
        public bool IsEnabled => uiText != null;

        public void Set(UIText text, float expireSeconds)
        {
            uiText = text;
            reduceCounter= new ReduceCounter((int)(1000 * expireSeconds));
            parent.RegisterCounter(reduceCounter);
            setTextSubject.OnNext(Unit.Default);
            reduceCounter.OnFinishedAsync.Subscribe(_ => textExpiredSubject.OnNext(Unit.Default));
        }

    }
}
