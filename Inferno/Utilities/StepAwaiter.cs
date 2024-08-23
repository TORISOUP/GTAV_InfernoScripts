using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Inferno.Utilities
{
    public class StepAwaiter : INotifyCompletion , IDisposable
    {
        private int _counter;
        private Action _continuation;
        private CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationTokenRegistration;
        
        public void Reset(int count, CancellationToken cancellationToken = default)
        {
            _counter = count;
            _cancellationToken = cancellationToken;

            // 前回のCancellationTokenRegistrationを解除
            _cancellationTokenRegistration.Dispose();

            // 新しいCancellationTokenに登録
            if (_cancellationToken.CanBeCanceled)
            {
                _cancellationTokenRegistration = _cancellationToken.Register(() =>
                {
                    // キャンセルされたら継続動作を即時実行
                    _counter = 0;
                    _continuation?.Invoke();
                });
            }
        }

        // Step()を呼ぶたびにカウントを1つ進める
        public void Step()
        {
            if (_counter > 0 && !_cancellationToken.IsCancellationRequested)
            {
                _counter--;
                if (_counter == 0)
                {
                    // カウントがゼロになったら継続動作を実行
                    _continuation?.Invoke();
                }
            }
        }

        public bool IsCompleted => _counter == 0 || _cancellationToken.IsCancellationRequested;

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        public void GetResult()
        {
            // キャンセルされている場合は例外を投げる
            _cancellationToken.ThrowIfCancellationRequested();
        }

        public void Dispose()
        {
            // CancellationTokenの登録を解除
            _cancellationTokenRegistration.Dispose();
        }
        
        public StepAwaiter GetAwaiter() => this;
    }
}