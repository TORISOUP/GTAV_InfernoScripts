﻿using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace Inferno
{
    /// <summary>
    ///　時限式テキスト
    /// </summary>
    internal class TimerUiTextManager
    {
        private InfernoScript parent;
        private ReduceCounter reduceCounter;
        private UIText uiText;
        private Subject<Unit> setTextSubject = new Subject<Unit>();
        public IObservable<Unit> OnSetTextAsObservable => setTextSubject.AsObservable();

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
