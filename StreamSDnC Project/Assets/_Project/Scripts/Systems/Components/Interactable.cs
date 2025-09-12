using Player.Weapons;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] string message;
    [field: SerializeField] public bool needsLineOfSight {  get; private set; }

    GameObject parent;

    public void Interact()
    {
        // call an event or somethin
        Debug.Log(message);
        InInteract();
    }

    protected abstract void InInteract(); // add a way to pass args nicely
}
