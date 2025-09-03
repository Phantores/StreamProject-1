using UnityEngine;

public class Interactable_Pickup : Interactable
{
    Rigidbody rb;
    GameObject isHeld;
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (GetComponent<Rigidbody>() == null) { Debug.LogWarning("A pickup interactable should have a Rigidbody!"); }
#endif
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if(isHeld) transform.position = isHeld.transform.position;
    }

    protected override void InInteract() { }

    public void ThrowObject(Vector3 direction, float force)
    {
        rb.AddForce(direction * force);
    }
}
