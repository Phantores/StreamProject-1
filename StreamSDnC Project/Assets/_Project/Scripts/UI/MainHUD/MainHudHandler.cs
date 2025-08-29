using UnityEngine.UIElements;
using UnityEngine;
using Player.Weapons;

namespace UI_Docs{
    public sealed class MainHudHandler : UI_Handler
    {
        [SerializeField] Color selectedColor;
        [SerializeField] Color unselectedColor;

        [SerializeField] Color selectedTextColor;
        [SerializeField] Color unselectedTextColor;

        Label mainWeaponLabel, sideWeaponLabel;
        Label clipAmmoLabel, reserveAmmoLabel;

        WeaponHandler.HoldState holdState = WeaponHandler.HoldState.None;

        protected override void InitUI()
        {
            mainWeaponLabel = root.Q<Label>("MainWeapon");
            sideWeaponLabel = root.Q<Label>("SideWeapon");

            clipAmmoLabel = root.Q<Label>("ClipAmmoDisplay");
            reserveAmmoLabel = root.Q<Label>("ReserveAmmoDisplay");

            UpdateColors();
            UpdateAmmo();
        }

        public void UpdateWeapon(Weapon weapon, bool which, WeaponHandler.HoldState changeTo, bool toggle = false, bool changeOverride = false)
        {
            if(!changeOverride) holdState = changeTo;

            if (changeTo != WeaponHandler.HoldState.None || changeOverride)
            {
                string newName = weapon == null ? "--" : weapon.data.Name;
                if(which)
                mainWeaponLabel.text = newName;
                else sideWeaponLabel.text = newName;
            }

            if (toggle)
            {
                UpdateColors();
                if(changeTo == WeaponHandler.HoldState.None)
                    UpdateAmmo();
                else
                    UpdateAmmo(weapon.currentAmmo, weapon.carriedAmmo);
            }
        }

        void UpdateColors()
        {
            switch(holdState)
            {
                case WeaponHandler.HoldState.None:
                    {
                        mainWeaponLabel.style.backgroundColor = unselectedColor;
                        sideWeaponLabel.style.backgroundColor = unselectedColor;

                        mainWeaponLabel.style.color = unselectedTextColor;
                        sideWeaponLabel.style.color = unselectedColor;
                        break;
                    }
                case WeaponHandler.HoldState.Main:
                    {
                        mainWeaponLabel.style.backgroundColor = selectedColor;
                        sideWeaponLabel.style.backgroundColor = unselectedColor;

                        mainWeaponLabel.style.color = selectedTextColor;
                        sideWeaponLabel.style.color = unselectedColor;
                        break;
                    }
                case WeaponHandler.HoldState.Side:
                    {
                        sideWeaponLabel.style.backgroundColor = selectedColor;
                        mainWeaponLabel.style.backgroundColor = unselectedColor;

                        sideWeaponLabel.style.unityTextOutlineColor = selectedTextColor;
                        mainWeaponLabel.style.unityTextOutlineColor = unselectedColor;
                        break;
                    }
                default:
                        {
                            mainWeaponLabel.style.backgroundColor = unselectedColor;
                            sideWeaponLabel.style.backgroundColor = unselectedColor;

                            mainWeaponLabel.style.color = unselectedTextColor;
                            sideWeaponLabel.style.color = unselectedColor;
                            break;
                        }
            }
        }

        public void UpdateAmmo(int clip = -5, int reserve = -5)
        {
            string clipString = clip <= -5 ? $"---" : $"{clip}";
            string reserveString = reserve <= -5 ? $"----" : $"{reserve}";

            clipAmmoLabel.text = clipString;
            reserveAmmoLabel.text = reserveString;
        }
    }
}
