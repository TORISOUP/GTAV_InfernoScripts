using System;
using System.Diagnostics;
using UniRx;

namespace Inferno.Utilities
{
    /// <summary>
    /// 指定したタイミングで実行されるスケジューラ
    /// </summary>
    public class InfernoScheduler : IScheduler
    {

        private object lockObject = new object();

        private SchedulerQueue schedulerQueue = new SchedulerQueue(4);
        private static Stopwatch stopWatch = Stopwatch.StartNew();

        private TimeSpan Time { get { return stopWatch.Elapsed; } }

        public IDisposable Schedule(Action action)
        {
            return Schedule(TimeSpan.Zero, action);
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            //実行する時間を決定
            var doTime = Time + Scheduler.Normalize(dueTime);
            var item = new ScheduledItem(action, doTime);

            lock (lockObject)
            {
                schedulerQueue.Enqueue(item);
            }

            return item.Cancellation;
        }

        public void Run()
        {
            lock (lockObject)
            {
                if (schedulerQueue.Count == 0) return;

                //登録されたアクションを実行
                while (schedulerQueue.Count > 0)
                {
                    //1個取り出す
                    var c = schedulerQueue.Peek();

                    if (c.IsCanceled)
                    {
                        //キャンセル済みなら破棄
                        schedulerQueue.Dequeue();
                        continue;
                    }

                    var wait = c.DueTime - Time;
                    if (wait.Ticks <= 0)
                    {
                        //実行可能状態なら実行
                        c.Invoke();
                        schedulerQueue.Dequeue();
                        continue;
                    }

                    //ここに到達する時は1つも処理できなかったとき
                    //つまり実行できるアクションがまだ無い
                    break;
                }
            }
        }

        public DateTimeOffset Now => Scheduler.Now;
    }
}
