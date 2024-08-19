using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace Inferno.Utilities
{
    public class InfernoScheduler : IScheduler
    {
        private readonly object _lockObject = new();
        private readonly PriorityQueue<ScheduledItem> _queue = new();

        public DateTimeOffset Now => DateTimeOffset.UtcNow;

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return Schedule(state, TimeSpan.Zero, action);
        }

        public IDisposable Schedule<TState>(TState state,
            TimeSpan dueTime,
            Func<IScheduler, TState, IDisposable> action)
        {
            return Schedule(state, Now.Add(dueTime), action);
        }

        public IDisposable Schedule<TState>(TState state,
            DateTimeOffset dueTime,
            Func<IScheduler, TState, IDisposable> action)
        {
            var scheduledItem = new ScheduledItem(this, state, dueTime, ObjAction);

            lock (_lockObject)
            {
                _queue.Enqueue(scheduledItem);
            }

            return Disposable.Create(() => scheduledItem.Cancel());

            IDisposable ObjAction(IScheduler scheduler, object objState)
            {
                return action(scheduler, (TState)objState);
            }
        }

        public void Run()
        {
            lock (_lockObject)
            {
                while (_queue.Count > 0)
                {
                    var next = _queue.Peek();
                    if (next.DueTime > Now) break;

                    _queue.Dequeue();
                    if (!next.IsCanceled) next.Invoke();
                }
            }
        }

        private class ScheduledItem : IComparable<ScheduledItem>
        {
            private readonly Func<IScheduler, object, IDisposable> _action;
            private readonly InfernoScheduler _scheduler;

            public ScheduledItem(InfernoScheduler scheduler,
                object state,
                DateTimeOffset dueTime,
                Func<IScheduler, object, IDisposable> action)
            {
                _scheduler = scheduler;
                _action = action;
                DueTime = dueTime;
                State = state;
            }

            public DateTimeOffset DueTime { get; }
            public bool IsCanceled { get; private set; }

            private object State { get; }

            public int CompareTo(ScheduledItem other)
            {
                return DueTime.CompareTo(other.DueTime);
            }

            public void Cancel()
            {
                IsCanceled = true;
            }

            public void Invoke()
            {
                if (!IsCanceled) _action(_scheduler, State);
            }
        }
    }
}