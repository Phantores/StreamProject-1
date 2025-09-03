using Player.Weapons;
using UnityEngine;

public class Interactable_Weapon : Interactable
{
    [field: SerializeField] public WeaponData Weapon { get; private set; }

    protected override void InInteract()
    {
        Destroy(this.gameObject);
    }
}
