using Player;
using UnityEngine;

public class Interactable_Pickup : Interactable
{
    Rigidbody rb;
    InteractionHandler owner;
    Vector3 target;
    Vector3 partialTarget;

    bool isRelativelyHeavy;

    bool thrown;

    Vector3 Dir()
    {
        Vector3 output = target - transform.position;
        output.Normalize();
        return output;
    }
    Vector3 ThrowDir()
    {
        Vector3 dir = owner._ctx.Data.holdOffset;
        dir.Normalize();
        return owner._ctx.Camera.transform.rotation * dir;
    }
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

    private void FixedUpdate()
    {
        if (owner == null || rb == null) return;
        if(thrown)
        {
            thrown = false;
            return;
        }
        partialTarget = owner._ctx.Camera.transform.rotation * owner._ctx.Data.holdOffset;
        target = owner._ctx.Camera.transform.position + VectorUtils.CappedSphereNormalize(partialTarget, owner._ctx.Data.holdOffset.z, owner._ctx.Data.holdHeightLimit);
        if(CheckDispose()) return;
        if (!isRelativelyHeavy)
        {
            rb.MovePosition(target);
        } else
        {
            rb.AddForce(Dir() * owner._ctx.Data.holdStrength);
        }
    }

    bool CheckDispose()
    {
        if (Vector3.Distance(rb.position, target) / owner._ctx.Data.holdDistanceLimit <= owner._ctx.Data.holdStrength / rb.mass)
        {
            return false;
        }
        owner.ForceDrop();
        Drop();
        return true;
    }

    protected override void InInteract() { }

    public void SetOwner(InteractionHandler ih)
    {
        owner = ih;
        isRelativelyHeavy = rb.mass > owner._ctx.Data.holdStrength;
        if(isRelativelyHeavy)
        {
            rb.useGravity = true;
        } else
        {
            rb.useGravity = false;
        }
    }

    public void Drop() { owner = null; rb.useGravity = true; Debug.Log("dropped"); }

    public void ThrowObject()
    {
        thrown = true;
        rb.useGravity = true;
        rb.AddForce(ThrowDir() * owner._ctx.Data.throwStrength, ForceMode.Impulse);
        owner = null;
    }
}
