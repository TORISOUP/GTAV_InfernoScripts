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
        /// カウンタを終了させる
        /// </summary>
        void Finish();
        /// <summary>
        /// カウントが正常にカウント完了したことを通知する
        /// </summary>
        IObservable<Unit> OnFinishedAsync { get; }
        /// <summary>
        /// タイマが完了状態であるか
        /// </summary>
        bool IsCompleted { get; }
    }
}