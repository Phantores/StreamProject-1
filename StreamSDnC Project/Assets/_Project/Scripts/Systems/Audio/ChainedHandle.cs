using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class ChainedHandle : Handle
    {
        readonly Queue<Event> eventQueue = new Queue<Event>();
        public Transform followTarget { get; }

        internal ChainedHandle(Emitter emitter, Transform target = null) : base(emitter)
        {
            this.followTarget = target;
        }

        public void AddToChain(Event audioEvent)
        {
            eventQueue.Enqueue(audioEvent);
        }

        public bool HasNext() => eventQueue.Count > 0;
        public Event GetNext() => eventQueue.Dequeue();

        public void FillChainFrom(Event startEvent)
        {
            var current = startEvent;
            while (current != null)
            {
                eventQueue.Enqueue(current);
                current = current.chainedEvent;
            }
        }
    }
}
