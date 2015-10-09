using System;
using System.Reactive;

namespace Inferno
{
    /// <summary>
    /// カウンタ
    /// </summary>
    public interface ICounter
    {
        /// <summary>
        /// カウンタの現在地
        /// </summary>
        int Current { get; }
        /// <summary>
        /// カウンタの現在の進行度
        /// </summary>
        float Rate { get; }
        /// <summary>
        /// カウンタを進行させる
        /// </summary>
        void Update(int countValue);
        /// <summary>
        /// カウント完了通知
        /// </summary>
        IObservable<Unit> OnFinishedAsync { get; }
    }
}