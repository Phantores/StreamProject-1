using UnityEngine;

namespace Audio{
    [CreateAssetMenu(fileName = "BusConfig", menuName = "Audio/BusConfig")]
    public class BusConfig : ScriptableObject
    {
        [field: SerializeField] public string sfxBus { get; private set; } = "bus:/SFX";
        [field: SerializeField] public string uiBus { get; private set; } = "bus:/UI";
        [field: SerializeField] public string ambienceBus { get; private set; } = "bus:/Ambience";
        [field: SerializeField] public string musicBus { get; private set; } = "bus:/Music";
    }
}
