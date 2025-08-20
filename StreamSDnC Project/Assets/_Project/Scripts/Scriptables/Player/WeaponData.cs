using UnityEditor.Animations;
using UnityEngine;

namespace Player{

    public enum FireMode
    {
        SemiAuto,
        Auto,
        Charged,
    }

    [CreateAssetMenu(fileName = "WeaponData", menuName = "Player/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; } = "New Weapon";
        [SerializeField] WeaponViewData viewData;
        [SerializeField] WeaponGameData gameData;
    
        public WeaponViewData ViewData => viewData;
        public WeaponGameData GameData => gameData;

        private void OnValidate()
        {
            if(gameData.recoil.Length != gameData.clipSize)
            {
                Debug.LogError($"Recoil array length ({gameData.recoil.Length}) does not match clip size ({gameData.clipSize}) in {name}.");
            }
        }
    }

    [System.Serializable]
    public class WeaponViewData
    {
        GameObject prefab;
        AnimatorController animatorController;
    }
    [System.Serializable]
    public struct WeaponGameData
    {
        public FireMode fireMode;

        public float chargeTime;
        public float fireRate;
        public float burstRate;
        public ushort burstCount;
        // charge attenuation

        public float damage;

        public int maxAmmo;
        public int clipSize;

        public Vector2[] recoil;
    }
}