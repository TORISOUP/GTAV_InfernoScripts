using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Inferno.Utilities
{
    /// <summary>
    /// 指定したタイミングで実行されるスケジューラ
    /// 雑に作ったので登録したイベントはキャンセルできない
    /// </summary>
    public class InfernoScheduler : IScheduler
    {
        private Queue<Action> actionQueue;

        private object lockObject = new object();

        public IDisposable Schedule(Action action)
        {
            return Schedule(TimeSpan.Zero, action);
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            lock (lockObject)
            {
                if (actionQueue == null)
                {
                    actionQueue = new Queue<Action>();
                }
                actionQueue.Enqueue(action);
            }

            return Disposable.Empty;
        }

        public void Run()
        {
            if (actionQueue == null) return;
            lock (lockObject)
            {
                while (actionQueue.Count > 0)
                {
                    actionQueue.Dequeue().Invoke();
                }
            }
        }

        public DateTimeOffset Now
        {
            get { return Scheduler.Now; }
        }

    }
}
