using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        public static IObservable<Unit> OnDrawingTickAsObservable => OnTickSubject.AsObservable();

        public DrawingCore()
        {
            Instance = this;

            Interval = 10;
            Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                .Select(_ => Unit.Default)
                .Multicast(OnTickSubject)
                .Connect();
        }
    }
}
