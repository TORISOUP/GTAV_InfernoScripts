using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

using UniRx;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{
    /// <summary>
    /// 減算カウンタ
    /// </summary>
    public class ReduceCounter : ICounter, IProgressBar
    {
        private readonly int _max;
        private readonly Subject<Unit> _onFinishedSubject;

        /// <summary>
        /// カウンタが正常にカウント終了した時にOnNextを発行する
        /// 強制終了時はOnCompletedのみ通知
        /// </summary>
        public UniRx.IObservable<Unit> OnFinishedAsync => _onFinishedSubject.AsObservable();
        public int Current { get; private set; }
        public float Rate => (float)Current/ (float)_max;
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// カウント回数を指定する
        /// AddCounterを利用する場合はミリ秒を与える
        /// </summary>
        public ReduceCounter(int count)
        {
            _max = count;
            Current = _max;
            _onFinishedSubject = new Subject<Unit>();
            IsCompleted = false;
        }


        public void Update(int countValue)
        {
            if (IsCompleted) return;

            Current = Current > countValue ? Current - countValue : 0;

            if (Current != 0) return;

            IsCompleted = true;
            _onFinishedSubject.OnNext(Unit.Default);
            _onFinishedSubject.OnCompleted();
        }

        public void Finish()
        {
            IsCompleted = true;
            _onFinishedSubject.OnCompleted();
        }

    }
}
