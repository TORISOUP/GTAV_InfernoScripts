using System.Collections.Concurrent;
using System.Threading;


namespace Inferno
{
    public class InfernoSynchronizationContext : SynchronizationContext
    {
        readonly ConcurrentQueue<TaskItem> _continuations = new ConcurrentQueue<TaskItem>();

        public override void Send(SendOrPostCallback d, object state)
        {
            _continuations.Enqueue(new TaskItem(d, state));
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _continuations.Enqueue(new TaskItem(d, state));
        }

        public void Update()
        {    
            while (_continuations.TryDequeue(out var i))
            {
                i.d(i.state);
            }
        }
    }

    internal struct TaskItem
    {
        public SendOrPostCallback d;
        public object state;

        public TaskItem(SendOrPostCallback d, object state)
        {
            this.d = d;
            this.state = state;
        }
    }
}
