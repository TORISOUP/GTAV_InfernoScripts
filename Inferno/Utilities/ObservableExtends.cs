using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.Utilities
{
    /// <summary>
    /// 情報通知のプロバイダーとしての機能を提供します。
    /// 引用 : http://blog.xin9le.net/entry/2012/01/04/111805
    /// </summary>
    /// <typeparam name="T">受信するデータの型</typeparam>
    class AnonymousObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> subscribe = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="subscribe">Subscribeのときに呼び出されるデリゲート</param>
        public AnonymousObservable(Func<IObserver<T>, IDisposable> subscribe)
        {
            if (subscribe == null)
                throw new ArgumentNullException("subscribe");
            this.subscribe = subscribe;
        }

        /// <summary>
        /// オブザーバーが通知を受け取ることをプロバイダーに通知します。
        /// </summary>
        /// <param name="observer">通知を受け取るオブジェクト</param>
        /// <returns>プロバイダーが通知の送信を完了する前に、オブザーバーが通知の受信を停止できるインターフェイスへの参照</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.subscribe(observer);
        }
    }

    public static class ObservableExtends
    {
        public static IObservable<TSource> ThrottleFirst<TSource>(this IObservable<TSource> source, TimeSpan dueTime)
        {
            return source.ThrottleFirst(dueTime, Scheduler.Immediate);
        }

        public static IObservable<TSource> ThrottleFirst<TSource>(this IObservable<TSource> source, TimeSpan dueTime,IScheduler scheduler)
        {
            return new AnonymousObservable<TSource>(observer =>
            {
                var gate = new object();
                var open = true;
                var cancelable = new SerialDisposable();

                var subscription = source.Subscribe(x =>
                {
                    lock (gate)
                    {
                        if (!open) return;
                        observer.OnNext(x);
                        open = false;
                    }

                    var d = new SingleAssignmentDisposable();
                    cancelable.Disposable = d;
                    d.Disposable = scheduler.Schedule(dueTime, () =>
                    {
                        lock (gate)
                        {
                            open = true;
                        }
                    });

                },
                    exception =>
                    {
                        cancelable.Dispose();

                        lock (gate)
                        {
                            observer.OnError(exception);
                        }
                    },
                    () =>
                    {
                        cancelable.Dispose();

                        lock (gate)
                        {
                            observer.OnCompleted();

                        }
                    });

                return new CompositeDisposable(subscription, cancelable);
            });
        }
    }
}
