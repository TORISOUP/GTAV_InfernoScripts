using System;
using System.Reactive.Subjects;
using Inferno.InfernoScripts.Event.Isono;
using Inferno.Utilities.Http;

namespace Inferno
{
    public sealed class IsonoHttpServer : IDisposable
    {
        private SimpleJsonHttpServer<IsonoMessage> _listener;
        private readonly int _port;
        public IObservable<IsonoMessage> OnReceivedMessageAsObservable => _subject;
        private readonly Subject<IsonoMessage> _subject = new();

        public IsonoHttpServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            if (_listener == null)
            {
                _listener = new SimpleJsonHttpServer<IsonoMessage>(_port);
                _listener.OnRequestReceived += message => _subject.OnNext(message);
            }

            _listener.Start();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        public void Dispose()
        {
            _listener?.Dispose();
            _subject?.OnCompleted();
            _subject?.Dispose();
        }
    }
}