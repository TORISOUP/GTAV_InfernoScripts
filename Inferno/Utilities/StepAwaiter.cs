using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Inferno.Utilities
{
    public class StepAwaiter : INotifyCompletion, IDisposable
    {
        private int _counter;
        private Action _continuation;
        private readonly CancellationTokenSource _internalCancellationTokenSource = new();
        private CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationTokenRegistration;
        private bool _disposed;
        private bool _isUsing;
        public bool IsActive => _isUsing && !_disposed;

        public void Reset(int count, CancellationToken cancellationToken = default)
        {
            _isUsing = true;
            _counter = count;
            _cancellationToken = CancellationTokenSource
                .CreateLinkedTokenSource(_internalCancellationTokenSource.Token, cancellationToken)
                .Token;

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

        public bool IsCompleted => _counter == 0 || _cancellationToken.IsCancellationRequested || _disposed;

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        public void GetResult()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StepAwaiter));
            // キャンセルされている場合は例外を投げる
            _cancellationToken.ThrowIfCancellationRequested();
        }

        public void Dispose()
        {
            _disposed = true;
            _isUsing = false;
            _internalCancellationTokenSource.Cancel();
            _internalCancellationTokenSource.Dispose();
            // CancellationTokenの登録を解除
            _cancellationTokenRegistration.Dispose();
        }

        public void Release()
        {
            _isUsing = false;
            _cancellationTokenRegistration.Dispose();
            _continuation = null;
        }

        public StepAwaiter GetAwaiter() => this;
    }
}