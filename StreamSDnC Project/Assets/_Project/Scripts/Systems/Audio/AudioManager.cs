using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio{
    public class AudioManager : Singleton<AudioManager>
    {
        readonly Dictionary<Event, List<Emitter>> activeEmitters = new();
        public readonly HashSet<Handle> liveHandles = new();

        #region Playback

        #region Playing

        public Handle PlayTracked(Event @event, Transform target) 
        {
            // 0. Nulls
            if (@event == null || target == null) return null;

            // 1. Get and check
            var emitter = PoolManager.Instance.GetEmitter(@event, target);
            if (emitter == null) return null;

            // 2. Register
            TrackEmitter(@event, emitter);

            // 3. Play
            emitter.transform.position = target.position;
            emitter.Play();

            // *4. Register handle
            var handle = new Handle(emitter);
            handle.SetOnDone(UnregisterHandle);
            RegisterHandle(handle);
            return handle;
        }
        public Handle PlayTracked(Event @event, Vector3 position)
        {
            // 0. Nulls
            if (@event == null) return null;

            // 1. Get and check
            var emitter = PoolManager.Instance.GetEmitter(@event);
            if (emitter == null) return null;

            // 2. Register
            TrackEmitter(@event, emitter);

            // 3. Play
            emitter.transform.position = position;
            emitter.Play();

            // *4. Register handle
            var handle = new Handle(emitter);
            handle.SetOnDone(UnregisterHandle);
            RegisterHandle(handle);
            return handle;
        }
        public Handle PlayTracked2D(Event @event)
        {
            // 0. Nulls
            if (@event == null) return null;

            // 1. Get and check
            var emitter = PoolManager.Instance.GetEmitter(@event);
            if (emitter == null) return null;

            // 2. Register
            TrackEmitter(@event, emitter);

            // 3. Play
            emitter.Play();

            // *4. Register handle
            var handle = new Handle(emitter);
            handle.SetOnDone(UnregisterHandle);
            RegisterHandle(handle);
            return handle;
        }

        public void PlayUntracked(Event @event, Transform target)
        {
            // 0. Nulls
            if (@event == null || target == null) return;

            // 1. Get and check
            var emitter = PoolManager.Instance.GetEmitter(@event, target);
            if (emitter == null) return;

            // 2. Register
            TrackEmitter(@event, emitter);

            // 3. Play
            emitter.transform.position = target.position;
            emitter.Play();
        }
        public void PlayUntracked(Event @event, Vector3 position)
        {
            // 0. Nulls
            if (@event == null) return;

            // 1. Get and check
            var emitter = PoolManager.Instance.GetEmitter(@event);
            if (emitter == null) return;

            // 2. Register
            TrackEmitter(@event, emitter);

            // 3. Play
            emitter.transform.position = position;
            emitter.Play();
        }
        public void PlayUntracked2D(Event @event)
        {
            // 0. Nulls
            if (@event == null) return;

            // 1. Get and check
            var emitter = PoolManager.Instance.GetEmitter(@event);
            if (emitter == null) return;

            // 2. Register
            TrackEmitter(@event, emitter);

            // 3. Play
            emitter.Play();
        }

        internal void PlayNext(ChainedHandle handle)
        {
            // 0. Check for next event and stop if none
            if (!handle.HasNext()) { handle.Stop(); return; }

            // 1. Get event and emitter
            Event nextEvent = handle.GetNext();
            var emitter = PoolManager.Instance.GetEmitter(nextEvent, handle.followTarget, handle);
            if (emitter == null) return;

            // 2. Play
            emitter.Play();
            handle.ChangeEmitter(emitter);
        }

        #endregion

        #region Stop

        public void StopAllTracked()
        {
            foreach (var handle in liveHandles.ToArray())
            {
                handle.Stop();
            }
            liveHandles.Clear();
        }
        public void StopAllUntracked(Event @event)
        {
            if (!activeEmitters.TryGetValue(@event, out var emitters)) return;
            foreach (var emitter in emitters.ToArray())
            {
                emitter.Stop();
            }
            activeEmitters.Remove(@event);
        }

        #endregion

        #endregion

        #region Registration

        public void TrackEmitter(Event @event, Emitter emitter)
        {
            if (!activeEmitters.ContainsKey(@event)) activeEmitters[@event] = new List<Emitter>();
            activeEmitters[@event].Add(emitter);
        }
        public void UntrackEmitter(Event @event, Emitter emitter)
        {
            if (activeEmitters.TryGetValue(@event, out var list))
            {
                list.Remove(emitter);
                if (list.Count == 0)
                    activeEmitters.Remove(@event);
            }
        }
        public void RegisterHandle(Handle handle) { if (!liveHandles.Contains(handle)) liveHandles.Add(handle); }
        public void UnregisterHandle(Handle handle) => liveHandles.Remove(handle);

        #endregion
    }
}
