using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public enum BusType
    {
        SFX, UI, Ambience, Music
    }
    public class MixerManager : Singleton<MixerManager> // this entire thing is yet to be checked, it's old and copypasted
    {
        [SerializeField] BusConfig busConfig;
        [SerializeField] List<Snapshot> preloadSnapshots;

        readonly Dictionary<BusType, Bus> buses = new();
        readonly Dictionary<Snapshot, EventInstance> snapshotCache = new();
        readonly Dictionary<Snapshot, EventInstance> activeSnapshots = new();

        #region BusRegistration

        void RegisterDefaultBuses()
        {
            RegisterBus(BusType.SFX, busConfig.sfxBus);
            RegisterBus(BusType.UI, busConfig.musicBus);
            RegisterBus(BusType.Ambience, busConfig.ambienceBus);
            RegisterBus(BusType.Music, busConfig.musicBus);
        }
        public void RegisterBus(BusType type, string path)
        {
            var bus = RuntimeManager.GetBus(path);

            if (bus.isValid()) buses[type] = bus;
            else { Debug.LogWarning($"FMOD bus path invalid or missing: {path}"); }
        }

        #endregion

        #region Volume

        public void SetVolume(BusType type, float volume)
        {
            if (buses.TryGetValue(type, out var bus)) bus.setVolume(volume);
        }
        public float GetVolume(BusType type)
        {
            if (buses.TryGetValue(type, out var bus))
            {
                bus.getVolume(out float volume);
                return volume;
            }
            return 0f;
        }

        #endregion

        #region Mute
        public void SetMute(BusType type, bool mute)
        {
            if (buses.TryGetValue(type, out var bus)) bus.setMute(mute);
        }
        public bool IsMuted(BusType type)
        {
            if (buses.TryGetValue(type, out var bus))
            {
                bus.getMute(out bool muted);
                return muted;
            }
            return false;
        }

        #endregion

        #region Snapshots

        void PreloadSnapshots()
        {
            foreach (var snapshot in preloadSnapshots)
            {
                if (snapshot == null) continue;
                var instance = RuntimeManager.CreateInstance(snapshot.snapshotEvent);
                snapshotCache[snapshot] = instance;
            }
        }

        public void ActivateSnapshot(Snapshot snapshot)
        {
            if (snapshot == null) return;
            if (activeSnapshots.ContainsKey(snapshot)) return;

            EventInstance instance;
            if (snapshotCache.TryGetValue(snapshot, out var cached))
            {
                instance = cached;
            }
            else
            {
                instance = RuntimeManager.CreateInstance(snapshot.snapshotEvent);
                snapshotCache[snapshot] = instance;
            }

            instance.start();
            activeSnapshots[snapshot] = instance;
        }
        public void DeactivateSnapshot(Snapshot snapshot)
        {
            if (snapshot == null) return;

            if (activeSnapshots.TryGetValue(snapshot, out var instance))
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
                activeSnapshots.Remove(snapshot);
            }
        }

        public void DeactivateAllSnapshots()
        {
            foreach (var kvp in activeSnapshots)
            {
                var instance = kvp.Value;
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
            }

            activeSnapshots.Clear();
        }

        public bool IsSnapshotActive(Snapshot snapshot)
        {
            return snapshot != null && activeSnapshots.ContainsKey(snapshot);
        }

        #endregion
    }
}
