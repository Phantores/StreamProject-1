using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class PoolManager : Singleton<PoolManager>
    {
        [SerializeField] GameObject emitterPrefab;
        [SerializeField] int maxPoolSize = 100;
        [SerializeField] float cleanupInterval = 60f;
        [SerializeField] float maxIdleTime = 300f;

        public int MaxPoolSize => maxPoolSize;

        readonly Dictionary<Guid, Pool> pools = new();
        readonly Dictionary<Guid, float> lastUsedTimestamps = new();

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(CleanupRoutine());
        }

        #region Registration

        internal void Register(Event audioEvent)
        {
            var guid = audioEvent.eventReference.Guid;
            if (!pools.ContainsKey(guid))
            {
                pools[guid] = new Pool(audioEvent, emitterPrefab, transform, audioEvent.PoolSize, maxPoolSize);
            }
        }
        internal void Unregister(Event audioEvent)
        {
            var guid = audioEvent.eventReference.Guid;
            if (pools.ContainsKey(guid))
            {
                pools.Remove(guid);
            }
        }

        #endregion

        #region EmitterAccess

        internal Emitter GetEmitter(Event audioEvent, Transform followTarget = null, Handle handle = null)
        {
            var guid = audioEvent.eventReference.Guid;

            if (!pools.ContainsKey(guid)) Register(audioEvent);

            var emitter = pools[guid].Get();
            lastUsedTimestamps[guid] = Time.time;
            if(emitter) emitter.Initialize(audioEvent, followTarget, handle);
            return emitter;
        }
        internal void ReturnEmitter(Event audioEvent, Emitter emitter)
        {
            var guid = audioEvent.eventReference.Guid;
            if (pools.TryGetValue(guid, out var pool))
            {
                pool.Return(emitter);
                lastUsedTimestamps[guid] = Time.time;
            }
        }

        #endregion

        #region Cleanup
        public float? GetLastUsedTime(Event audioEvent)
        {
            var guid = audioEvent.eventReference.Guid;
            if (lastUsedTimestamps.TryGetValue(guid, out float timestamp)) return timestamp;
            return null;
        }

        private IEnumerator CleanupRoutine()
        {
            var wait = new WaitForSeconds(cleanupInterval);
            while (true)
            {
                yield return wait;

                List<Guid> toRemove = new();

                foreach (var kvp in lastUsedTimestamps)
                {
                    if (Time.time - kvp.Value > maxIdleTime) toRemove.Add(kvp.Key);
                }
                foreach (var guid in toRemove)
                {
                    if (pools.ContainsKey(guid))
                    {
                        pools.Remove(guid);
                        lastUsedTimestamps.Remove(guid);
                    }
                }
            }
        }

        #endregion
    }
}
