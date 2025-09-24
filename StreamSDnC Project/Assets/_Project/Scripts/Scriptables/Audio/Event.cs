using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace Audio{
    [CreateAssetMenu(fileName = "Event", menuName = "Audio/Event")]
    public class Event : ScriptableObject
    {
        [field: SerializeField] public EventReference eventReference { get; private set; }

        [field: SerializeField] public bool Is3D { get; private set; }
        [field: SerializeField] public bool IsLooping { get; private set; }
        [field: SerializeField] public int PoolSize { get; private set; }
        [field: SerializeField] public Event chainedEvent { get; private set; }

        [System.Serializable]
        public class AudioParameter
        {
            public string Name;
            public float Value;
        }

        [SerializeField] private List<AudioParameter> defaultParameters = new();
        public List<AudioParameter> DefaultParameters => defaultParameters;

        private Dictionary<string, float> defaultParameterCache;

        void CacheDefaultParameters()
        {
            defaultParameterCache = new Dictionary<string, float>();
            foreach(var param in defaultParameters)
            {
                if (!string.IsNullOrEmpty(param.Name)) defaultParameterCache[param.Name] = param.Value;
            }
        }

        public float? GetDefaultParamValue(string paramName)
        {
            if(defaultParameterCache != null && defaultParameterCache.TryGetValue(paramName, out var value)) return value;
            return null;
        }

        private void OnEnable()
        {
            CacheDefaultParameters();
        }
        private void OnValidate()
        {
            CacheDefaultParameters();
            if(!eventReference.IsNull && PoolSize <= 0)
            {
                UnityEngine.Debug.LogWarning($"AudioEvent '{name}' has a non-null event but PoolSize is 0 or negative. Defaulting to 10.");
                PoolSize = 10;
            }
            if(eventReference.IsNull)
            {
                UnityEngine.Debug.LogWarning($"AudioEvent '{name}' is missing an EventReference.");
            }
        }
    }
}
