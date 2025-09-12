using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace Level{
    public class ZoneRegistry : SceneSingleton<ZoneRegistry>
    {
        [field: SerializeField] public List<Zone> zones { get; private set; } = new List<Zone>();
        #region Registration
        void Start()
        {
            LevelManager.Instance.RegisterZone(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelManager.Instance.UnregisterZone(this);
        }

        public void RegisterZone(Zone zone)
        {
            if(!zones.Contains(zone)) zones.Add(zone);
        }

        public void UnregisterZone(Zone zone)
        {
            zones.Remove(zone);
        }
        #endregion

        struct ZoneData
        {
            public float3 position;
            public float simulationRange;
            public bool activeState;
            public bool isCaller;
        }
        NativeArray<ZoneData> zonesNative;

        public void UpdateState(bool enable, Zone caller)
        {
            if (caller == null || !zones.Contains(caller))
            {
                UnityEngine.Debug.LogError($"{caller} is not present in the list.");
                return;
            }

            Zone[] zoneArray = zones.ToArray();
            zonesNative = new NativeArray<ZoneData>(zones.Count, Allocator.TempJob);
            for (int i = 0; i < zoneArray.Length; i++)
            {
                zonesNative[i] = new ZoneData
                {
                    position = zoneArray[i].transform.position,
                    simulationRange = zoneArray[i].simulationRange,
                    activeState = zoneArray[i].gameObject.activeSelf,
                    isCaller = zoneArray[i] == caller
                };
            }

            ZoneData callerData = new ZoneData
            {
                position = caller.transform.position,
                simulationRange = caller.simulationRange,
                activeState = caller.gameObject.activeSelf
            };

            var updateJob = new UpdateZonesJob { zones = zonesNative, mode = enable, caller = callerData };
            JobHandle updateJobHandle = updateJob.Schedule(zonesNative.Length, 32);

            updateJobHandle.Complete();

            for (int i = 0; i < zonesNative.Length; i++)
            {
                zones[i].gameObject.SetActive(zonesNative[i].activeState);
            }

            zonesNative.Dispose();
        }

        [BurstCompile]
        struct UpdateZonesJob : IJobParallelFor
        {
            public NativeArray<ZoneData> zones;
            public ZoneData caller;
            public bool mode;
            public void Execute(int index)
            {
                ZoneData zone = zones[index];
                if (zone.isCaller) return;
                if(mode)
                {
                    if (sqrMag(caller.position - zone.position) <= caller.simulationRange * caller.simulationRange)
                    {
                        zone.activeState = mode;
                    }
                } else
                {
                    if (sqrMag(caller.position - zone.position) > caller.simulationRange * caller.simulationRange)
                    {
                        zone.activeState = mode;
                    }
                }
                    zones[index] = zone;
            }

            float sqrMag(float3 vec)
            {
                return (vec.x * vec.x) + (vec.y * vec.y) + (vec.z * vec.z);
            }
        }
    }
}
