using UnityEngine;

namespace Player.Weapons{
    public class WeaponCenter : MonoBehaviour
    {
        public ModelController weaponModel {  get; private set; }
        Animator animator => this.gameObject.GetComponent<Animator>();

        public void Aim(bool aim, float ads = 1)
        {
            animator.SetBool("Aiming", aim);
        }

        public void ChangeWeapon(ModelController newPrefab)
        {
            if(weaponModel != null) Destroy(weaponModel.gameObject);

            weaponModel = Instantiate(newPrefab, this.transform);
        }

        public void ChangeWeapon(Weapon newWeapon)
        {
            if (weaponModel != null) Destroy(weaponModel.gameObject);

            weaponModel = Instantiate(newWeapon.data.ViewData.prefab, this.transform);
        }

        public void DisposeWeapon()
        {
            Destroy(weaponModel.gameObject);
            weaponModel = null;
        }
    }
}
