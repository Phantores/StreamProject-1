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

        public bool Reloading { get; private set; }

        Weapon mainWeapon;
        Weapon sideWeapon;

        CameraController _cameraCont;
        WeaponCenter _center;

        public WeaponHandler(CameraController camera, WeaponCenter center)
        {
            _cameraCont = camera;
            _center = center;
        }

        #region WeaponSetters

        public void PickWeapon(HoldState holdState)
        {
            switch (_holdState)
            {
                case HoldState.Main:
                    if(mainWeapon != null) holdState = HoldState.Main;
                    _center.ChangeWeapon(mainWeapon);
                    break;
                case HoldState.Side:
                    if (sideWeapon != null) holdState = HoldState.Side;
                    _center.ChangeWeapon(sideWeapon);
                    break;
                default:
                    holdState = HoldState.None;
                    _center.DisposeWeapon();
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

        #region ShootingMethods

        internal void Shoot(float time, Weapon weapon)
        {
            _center.weaponModel.ScheduleAnimation(ModelController.AnimationMode.Shooting);
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
            return GetCurrentWeapon()?.BurstCoroutine() ?? Task.CompletedTask;
        }

        public Task ReloadWeapon(CancellationToken ct)
        {
            return GetCurrentWeapon()?.ReloadCoroutine(ct) ?? Task.CompletedTask;
        }

        internal void ChangeReloadState(bool state) => Reloading = state;

        #endregion

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

        bool _softReloadCancel;

        public Weapon(WeaponData weaponData, WeaponHandler Parent)
        {
            data = weaponData;
            wh = Parent;
        }

        public void SetData(WeaponData weaponData)
        {
            data = weaponData;
        }

        void Reload() // this needs to be changed
        {
            if (gameData.reloadPortion == null)
            {
                int ammoNeeded = data.GameData.clipSize - currentAmmo;
                if (carriedAmmo >= ammoNeeded)
                {
                    int ammoToReload = carriedAmmo % ammoNeeded;
                    currentAmmo += ammoToReload;
                    carriedAmmo -= ammoToReload;
                }
            } else
            {
                
            }
        }

        bool ReloadEval()
        {
            if (carriedAmmo == 0) return false;
            if (currentAmmo == gameData.maxAmmo) return false;


            return true;
        }

        void TryShoot()
        {
            if (currentAmmo > 0)
            {
                currentAmmo--;
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
            await CoroutineRunner.Instance.DelayScaled(gameData.fireRate - gameData.burstRate);      //Wait additional `delay` time
            firedOnce = false;
        }

        public async Task ReloadCoroutine(CancellationToken ct)
        {
            wh.ChangeReloadState(true);
            _softReloadCancel = false;

            try
            {
                if(gameData.reloadPortion == null)
                {
                    await CoroutineRunner.Instance.DelayScaled(gameData.reloadTime);
                    if (_softReloadCancel) return;
                    Reload();
                }
                else
                {
                    while(ReloadEval())
                    {
                        if (_softReloadCancel) break;
                        ct.ThrowIfCancellationRequested();

                        await CoroutineRunner.Instance.DelayScaled(gameData.reloadTime, ct);

                        if (_softReloadCancel) break;
                        if(ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();

                        Reload();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // cleanup
            }
            finally
            {
                wh.ChangeReloadState(false);
            }

        }
    }
}
