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

        public PlayerContext _ctx { get; private set; }

        CameraController _cameraCont => _ctx.Camera;
        WeaponCenter _center => _ctx.WeaponCenter;

        public WeaponHandler(PlayerContext ctx)
        {
            _ctx = ctx;
        }

        #region WeaponSetters

        public void PickWeapon(HoldState holdState)
        {
            switch (holdState)
            {
                case HoldState.Main:
                    if (mainWeapon == null) return;
                    _holdState = HoldState.Main;
                    _center.ChangeWeapon(mainWeapon);
                    _ctx.Runner.mainHud.UpdateWeapon(mainWeapon, true, HoldState.Main, true);
                    break;
                case HoldState.Side:
                    if (sideWeapon == null) return;
                    _holdState = HoldState.Side;
                    _center.ChangeWeapon(sideWeapon);
                    _ctx.Runner.mainHud.UpdateWeapon(sideWeapon, false, HoldState.Side, true);
                    break;
                default:
                    _holdState = HoldState.None;
                    _center.DisposeWeapon();
                    _ctx.Runner.mainHud.UpdateWeapon(mainWeapon, true, HoldState.None, true);
                    break;
            }
        }

        public void SetWeapon(bool main, WeaponData weaponData)
        {
            Debug.Log($"Changed weapon to {weaponData}");
            Weapon weapon = main ? mainWeapon : sideWeapon;
            if (weaponData == null) weapon = null;
            else if(weapon == null)
            {
                weapon = new Weapon(weaponData, this);
            }
            else
            {
                weapon = new Weapon(weaponData, this);
            }

            if (main) mainWeapon = weapon;
            else sideWeapon = weapon;
            _ctx.Runner.mainHud.UpdateWeapon(weapon, main, _holdState, false, true);
        }

        public void CollectWeapon(WeaponData weaponData)
        {
            if(weaponData == null) return;

            bool where = true;
            if (mainWeapon == null) where = true;
            else if (sideWeapon == null) where = false;
            else
            {
                switch (_holdState)
                {
                    case HoldState.Main:
                        {
                            where = true;
                            break;
                        }
                    case HoldState.Side:
                        {
                            where = false;
                            break;
                        }
                    default:
                        {
                            where = true;
                            break;
                        }
                }
            }

            SetWeapon(where, weaponData);

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

        public bool IsWeaponNull(HoldState holdState)
        {
            switch (holdState)
            {
                case HoldState.Main:
                    return mainWeapon == null;
                case HoldState.Side:
                    return sideWeapon == null;
                default:
                    return false;
            }
        }
        #endregion

        #region ShootingMethods

        internal void Shoot(float time, Weapon weapon, Vector2 recoil)
        {
            _center.weaponModel.ScheduleAnimation(ModelController.AnimationMode.Shooting, weapon.data.GameData.fireTime);
            Ray ray = _cameraCont.Camera.ScreenPointToRay(InputManager.Instance.MousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                HealthComponent hitentity = hit.transform.gameObject.GetComponent<HealthComponent>();
                if(hitentity != null)
                {
                    hitentity.Damage(weapon.data.GameData.damage);
                }
            }
            _cameraCont.PassRotation(recoil);
            _ctx.Runner.mainHud.UpdateAmmo(weapon.currentAmmo, weapon.reserveAmmo);
        }

        internal void Reload(Weapon weapon)
        {
            _center.weaponModel.ScheduleAnimation(ModelController.AnimationMode.Reloading, weapon.data.GameData.reloadTime);
        }
        internal void Cancel()
        {
            _center.weaponModel.ScheduleAnimation(ModelController.AnimationMode.Cancel);
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
        public int reserveAmmo { get; private set; } = 0;

        WeaponGameData gameData => data.GameData;

        bool firedOnce;
        bool skipDelay;
        bool _softReloadCancel;
        internal float chargeTimeStamp;

        WeaponHandler wh;

        public Weapon(WeaponData weaponData, WeaponHandler Parent)
        {
            data = weaponData;
            wh = Parent;

            currentAmmo = gameData.clipSize;
            reserveAmmo = gameData.maxAmmo / 2;
        }

        public void SetData(WeaponData weaponData)
        {
            data = weaponData;
        }

        void Reload()
        {
            int space = Math.Max(0, gameData.clipSize - currentAmmo);
            int portion = gameData.reloadPortion.HasValue ? gameData.reloadPortion.Value : space;
            int toLoad = Math.Min(space, Math.Min(portion, reserveAmmo));

            if (toLoad < 0) toLoad = 0;

            reserveAmmo -= toLoad;
            currentAmmo += toLoad;

            wh._ctx.Runner.mainHud.UpdateAmmo(currentAmmo, reserveAmmo);
        }

        public bool ReloadEval() // make sure is complete
        {
            if (reserveAmmo <= 0) return false;
            if (currentAmmo >= gameData.clipSize) return false;


            return true;
        }

        bool TryShoot()
        {
            if (currentAmmo > 0)
            {
                currentAmmo--;
                wh.Shoot(Time.time - chargeTimeStamp, this, gameData.recoil[currentAmmo]);
                return true;
            }
            else if (reserveAmmo > 0 && !firedOnce)
            {
                // idk prompt to reload or something
                return false;
            }
            else
            {
                // do stuff for empty gun
                return false;
            }
        }

        public async Task BurstCoroutine()
        {
            skipDelay = false;

                for (int i = 0; i < gameData.burstCount; i++)
                {
                    if (!TryShoot()) { skipDelay = true; break;}
                    firedOnce = true;
                    await CoroutineRunner.Instance.DelayScaled(gameData.burstRate);
                }
                firedOnce = false;
                if (!skipDelay) await CoroutineRunner.Instance.DelayScaled(gameData.fireRate - gameData.burstRate);

        }

        public async Task ReloadCoroutine(CancellationToken ct)
        {
            wh.ChangeReloadState(true);
            _softReloadCancel = false;

            try
            {
                if(!gameData.reloadPortion.HasValue && ReloadEval())
                {
                    wh.Reload(this);
                    await CoroutineRunner.Instance.DelayScaled(gameData.reloadTime * gameData.reloadAttenuation.Evaluate(currentAmmo / gameData.clipSize), ct);
                    ct.ThrowIfCancellationRequested(); // fixed this
                    Reload();
                }
                else if(gameData.reloadPortion.HasValue)
                {
                    while(ReloadEval())
                    {
                        if (_softReloadCancel)
                        {
                            wh.Cancel();
                            break;
                        }
                        ct.ThrowIfCancellationRequested();

                        wh.Reload(this);
                        await CoroutineRunner.Instance.DelayScaled(gameData.reloadTime, ct);

                        if (_softReloadCancel)
                        {
                            wh.Cancel();
                            break;
                        }
                        ct.ThrowIfCancellationRequested();

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
                wh.Cancel();
            }

        }
    }
}
