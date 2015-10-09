using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        public IObservable<Unit> OnFinishedAsync => _onFinishedSubject.AsObservable();
        public int Current { get; private set; }
        public float Rate => (float) Current/_max;
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// カウント回数を指定する
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
    }
}
