using System;

namespace Player{
    public class WeaponHandler
    {
        public enum HoldState { None, Main, Side}
        HoldState _holdState = HoldState.None;

        Weapon mainWeapon;
        Weapon sideWeapon;

        int mainBullets = 0;
        int sideBullets = 0;

        public WeaponHandler()
        {
            
        }

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
