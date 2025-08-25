using UnityEditor.Animations;
using UnityEngine;

namespace Player.Weapons{

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
        [field: SerializeField] public ModelController prefab {  get; private set; }
        [field: SerializeField] public AnimatorController animatorController;
    }
    [System.Serializable]
    public struct WeaponGameData
    {
        public FireMode fireMode;

        public float chargeTime;
        public float fireRate;
        public float burstRate;
        public int burstCount;

        public float damage;

        public int maxAmmo;
        public int clipSize;

        public int? reloadPortion; //if null then reload all at once
        public float reloadTime;

        public Vector2[] recoil;

        public float weightMultiplier;

        //Charge attenuation
        //Range attenuation
    }
}