using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace Inferno.Utilities
{
    public static class ObservableExt
    {
        public static IObservable<TSource> ThrottleFirst<TSource>(this IObservable<TSource> source,
            TimeSpan dueTime,
            IScheduler scheduler)
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
                            if (!open)
                            {
                                return;
                            }

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