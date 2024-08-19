using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using System;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;

namespace Inferno
{
    /// <summary>
    /// 描画用のOnTickを提供する
    /// </summary>
    internal class DrawingCore : Script
    {
        public static DrawingCore Instance { get; private set; }

        private static readonly Subject<Unit> OnTickSubject = new Subject<Unit>();

        public static IObservable<Unit> OnDrawingTickAsObservable => OnTickSubject.AsObservable();

        public DrawingCore()
        {
            Instance = this;

            Interval = 0;
            Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                .Select(_ => Unit.Default)
                .Multicast(OnTickSubject)
                .Connect();
        }
    }
}
