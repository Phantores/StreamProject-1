using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class Pool
    {
        readonly Queue<Emitter> pool = new();
        readonly GameObject prefab;
        readonly Transform parent;
        readonly Event audioEvent;
        readonly int maxSize;

        int totalInstances;

        public int PoolCount => pool.Count;

        public Pool(Event audioEvent, GameObject emitterPrefab, Transform parent, int size, int maxSize)
        {
            this.audioEvent = audioEvent;
            this.parent = parent;
            this.maxSize = maxSize;
            this.prefab = emitterPrefab;
            this.totalInstances = 0;

            for (int i = 0; i < size; i++)
            {
                CreateEmitter();
            }
        }

        public Emitter Get()
        {
            if (maxSize > 0 && totalInstances >= maxSize) return null;
            if (pool.Count == 0) CreateEmitter();

            var newEmitter = pool.Dequeue();
            newEmitter.gameObject.SetActive(true);
            return newEmitter;
        }
        public void Return(Emitter emitter)
        {
            emitter.gameObject.SetActive(false);
            pool.Enqueue(emitter);
        }

        void CreateEmitter()
        {
            var obj = Object.Instantiate(prefab, parent);
            var emitter = obj.GetComponent<Emitter>();
            emitter.gameObject.SetActive(false);
            pool.Enqueue(emitter);
            totalInstances++;
        }

    }
}
