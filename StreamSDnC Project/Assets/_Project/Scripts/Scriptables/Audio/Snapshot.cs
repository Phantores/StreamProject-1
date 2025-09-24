using UnityEngine;
using FMODUnity;

namespace Audio{
    [CreateAssetMenu(fileName = "Snapshot", menuName = "Audio/Snapshot")]
    public class Snapshot : ScriptableObject
    {
        [field: SerializeField] public EventReference snapshotEvent { get; private set; }
    }
}
