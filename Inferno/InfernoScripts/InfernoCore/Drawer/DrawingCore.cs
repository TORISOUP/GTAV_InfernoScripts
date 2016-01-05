using System;
using UniRx;

using UniRx;
using GTA; using UniRx;

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
        public static UniRx.IObservable<Unit> OnDrawingTickAsObservable => OnTickSubject.AsObservable();

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
