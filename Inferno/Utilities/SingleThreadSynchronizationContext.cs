using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inferno.Utilities
{
    /// <summary>
    /// http://qiita.com/ousttrue/items/66def43267329bc132ff
    /// より引用
    /// </summary>
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly
            BlockingCollection<KeyValuePair<SendOrPostCallback, object>>
            m_queue =
                new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            m_queue.Add(
                new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public void RunOnCurrentThread()
        {
            if(m_queue.Count==0) return;
            KeyValuePair<SendOrPostCallback, object> workItem;
            while (m_queue.TryTake(out workItem, 20))
                workItem.Key(workItem.Value);
        }

        public void Complete()
        {
            m_queue.CompleteAdding();
        }

    }

}
