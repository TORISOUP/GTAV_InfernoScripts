using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Inferno.Utilities
{
    /// <summary>
    /// http://qiita.com/ousttrue/items/66def43267329bc132ff
    /// より引用
    /// </summary>
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly
            ConcurrentQueue<KeyValuePair<SendOrPostCallback, object>>
            _mQueue = new();

        public override void Post(SendOrPostCallback d, object state)
        {
            _mQueue.Enqueue(
                new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public void RunOnCurrentThread()
        {
            if (_mQueue.Count == 0)
            {
                return;
            }

            KeyValuePair<SendOrPostCallback, object> workItem;
            while (_mQueue.TryDequeue(out workItem))
                workItem.Key(workItem.Value);
        }

        public void Complete()
        {
        }
    }
}