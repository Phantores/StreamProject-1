using UnityEngine;

namespace Player.Weapons{
    public class WeaponCenter : MonoBehaviour
    {
        public ModelController weaponModel {  get; private set; }

        public void ChangeWeapon(ModelController newPrefab)
        {
            if(weaponModel != null) Destroy(weaponModel.gameObject);

            weaponModel = Instantiate(newPrefab);
        }

        public void ChangeWeapon(Weapon newWeapon)
        {
            if (weaponModel != null) Destroy(weaponModel.gameObject);

            weaponModel = Instantiate(newWeapon.data.ViewData.prefab);
        }

        public void DisposeWeapon()
        {
            Destroy(weaponModel);
            weaponModel = null;
        }
    }
}
