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

        [field: SerializeField] public Audio.Event gunshotSound { get; private set; }
    }
    [System.Serializable]
    public class WeaponGameData
    {
        public FireMode fireMode;

        public float chargeTime;
        public float fireRate;
        public float burstRate;
        public int burstCount;

        public float fireTime => burstRate * burstCount + fireRate;

        public float damage;

        public int maxAmmo;
        public int clipSize;

        [SerializeField] public NullableInt reloadPortion; //if null then reload all at once
        public float reloadTime;

        public Vector2[] recoil;

        public float weightMultiplier;
        public float aimSpeedMultiplier;
        public float aimTime;

        public AnimationCurve chargeAttenuation = AnimationCurve.Constant(0, 1, 1);
        public AnimationCurve reloadAttenuation = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve rangeAttenuation = AnimationCurve.Constant(0, 1, 1);

        //Charge attenuation
        //Range attenuation
    }
}
