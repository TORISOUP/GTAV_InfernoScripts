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
    internal class DrawingCore : Script
    {
        private static readonly Subject<Unit> OnTickSubject = new();
        private IDisposable _disposable;

        public DrawingCore()
        {
            Instance = this;

            Interval = 0;
            _disposable = Observable
                .FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                .Select(_ => Unit.Default)
                .Multicast(OnTickSubject)
                .Connect();
        }

        public static DrawingCore Instance { get; private set; }

        public static IObservable<Unit> OnDrawingTickAsObservable => OnTickSubject.AsObservable();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposable?.Dispose();
            }
        }
    }
}