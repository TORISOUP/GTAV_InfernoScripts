using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace Inferno
{
    /// <summary>
    /// 描画用のOnTickを提供する
    /// </summary>
    class DrawingCore : Script
    {
        public static DrawingCore Instance { get; private set; }


        private static readonly Subject<Unit> OnTickSubject = new Subject<Unit>();
        /// <summary>
        /// 100ms周期のTick
        /// </summary>
        public static IObservable<Unit> OnDrawingTickAsObservable
        {
            get { return OnTickSubject.AsObservable(); }
        }

        public DrawingCore()
        {
            Instance = this;

            Interval = 0;
            Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h, Scheduler.Immediate)
                .Select(_ => Unit.Default)
                .Multicast(OnTickSubject)
                .Connect();

        }
    }
}
