using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace EventSystem{
    public static class EventBusUtil
    {
        public static IReadOnlyList<Type> EventTypes { get; set; }
        public static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; set; }
        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            PlayModeState = state;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                ClearAllBuses();
            }
        }

#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
            EventBusTypes = InitializeAllBuses();
        }

        static List<Type> InitializeAllBuses()
        {
            List<Type> eventBusTypes = new List<Type>();
            var typedef = typeof(EventBus<>);
            foreach(var eventType in EventTypes)
            {
                var busType = typedef.MakeGenericType(eventType);
                eventBusTypes.Add(busType);
            }

            return eventBusTypes;
        }

        public static void ClearAllBuses()
        {
            for(int i = 0; i < EventBusTypes.Count; i++)
            {
                var busType = EventBusTypes[i];
                var method = busType.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
                method?.Invoke(null, null);
            }
        }
    }
}
