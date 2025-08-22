using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Player{
    public class WeaponHandler
    {
        public enum HoldState { None = 0, Main = 1, Side = 2}
        HoldState _holdState = HoldState.None;

        Weapon mainWeapon;
        Weapon sideWeapon;

        CameraController _cameraCont;

        public WeaponHandler(CameraController camera)
        {
            _cameraCont = camera;
        }
        #region WeaponSetters

        public void PickWeapon(HoldState holdState)
        {
            switch (_holdState)
            {
                case HoldState.Main:
                    if(mainWeapon != null) holdState = HoldState.Main;
                    break;
                case HoldState.Side:
                    if (sideWeapon != null) holdState = HoldState.Side;
                    break;
                default:
                    holdState = HoldState.None;
                    break;
            }
        }

        public void SetWeapon(bool main, WeaponData weapon)
        {
            if(main) mainWeapon.SetData(weapon);
            else sideWeapon.SetData(weapon);
        }

        public Weapon GetCurrentWeapon()
        {
            switch(_holdState)
            {
                case HoldState.Main:
                    if (mainWeapon != null) return mainWeapon;
                    break;
                case HoldState.Side:
                    if (sideWeapon != null) return sideWeapon;
                    break;
                default:
                    return null;
            }
            return null;
        }
        #endregion

        public void Shoot(float time, Weapon weapon)
        {
            Ray ray = _cameraCont.Camera.ScreenPointToRay(InputManager.Instance.MousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                HealthComponent hitentity = hit.transform.gameObject.GetComponent<HealthComponent>();
                if(hitentity != null)
                {
                    hitentity.Damage(weapon.data.GameData.damage);
                }
            }
        }
    }

    public class Weapon
    {
        public WeaponData data { get; private set; }
        public int currentAmmo { get; private set; } = 0;

        public int carriedAmmo { get; private set; } = 0;
        public Weapon(WeaponData weaponData)
        {
            data = weaponData;
        }

        public void SetData(WeaponData weaponData)
        {
            data = weaponData;
        }

        public void Reload()
        {
            int ammoNeeded = data.GameData.clipSize - currentAmmo;
            if (carriedAmmo >= ammoNeeded)
            {
                int ammoToReload = carriedAmmo % ammoNeeded;
                currentAmmo += ammoToReload;
                carriedAmmo -= ammoToReload;
            }
        }
    }
}
