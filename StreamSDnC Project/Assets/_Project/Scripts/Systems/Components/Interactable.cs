using Player.Weapons;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] string message;

    Rigidbody rb;

    public void Interact()
    {
        // call an event or somethin
        Debug.Log(message);
        InInteract();
    }

    protected abstract void InInteract(); // add a way to pass args nicely

    public void Drop() { } // move to Pickup
}
