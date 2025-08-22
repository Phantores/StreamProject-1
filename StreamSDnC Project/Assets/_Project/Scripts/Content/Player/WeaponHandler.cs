using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Player.Weapons{
    public class WeaponHandler
    {
        const float timecheck = -526278;
        public enum HoldState { None = 0, Main = 1, Side = 2}
        public HoldState _holdState { get; private set; } = HoldState.None;

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

        public Task ShootWeapon(float timestamp = timecheck)
        {
            if(timestamp != timecheck) GetCurrentWeapon().chargeTimeStamp = timestamp;
            return GetCurrentWeapon().BurstCoroutine();
        }

    }

    public class Weapon
    {
        public WeaponData data { get; private set; }
        public int currentAmmo { get; private set; } = 0;
        public int carriedAmmo { get; private set; } = 0;

        WeaponGameData gameData => data.GameData;

        bool firedOnce;

        internal float chargeTimeStamp;

        WeaponHandler wh;

        public Weapon(WeaponData weaponData, WeaponHandler Parent)
        {
            data = weaponData;
            wh = Parent;
        }

        public void SetData(WeaponData weaponData)
        {
            data = weaponData;
        }

        void Reload()
        {
            int ammoNeeded = data.GameData.clipSize - currentAmmo;
            if (carriedAmmo >= ammoNeeded)
            {
                int ammoToReload = carriedAmmo % ammoNeeded;
                currentAmmo += ammoToReload;
                carriedAmmo -= ammoToReload;
            }
        }

        void TryShoot()
        {
            if (currentAmmo > 0)
            {
                wh.Shoot(Time.time - chargeTimeStamp, this);
            }
            else if (carriedAmmo > 0 && !firedOnce)
            {
                Reload();
            }
            else
            {
                // do stuff
            }
        }

        public async Task BurstCoroutine()
        {
            for (int i = 0; i < gameData.burstCount; i++)                        //Repetively call Shoot() every `time` seconds
            {
                TryShoot();
                firedOnce = true;
                await CoroutineRunner.Instance.DelayScaled(gameData.burstRate);
            }
            await CoroutineRunner.Instance.DelayScaled(gameData.fireRate);      //Wait additional `delay` time
            firedOnce = false;
        }
    }
}
