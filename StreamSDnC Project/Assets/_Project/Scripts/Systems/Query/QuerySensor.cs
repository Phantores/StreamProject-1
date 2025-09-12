using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class QuerySensor : MonoBehaviour
{
    public readonly HashSet<Targetable> _set = new();

    [SerializeField] LayerMask targetLayers;
    [SerializeField] float radius = 25f;
    [SerializeField] float halfAngleDeg = 30f;
    [SerializeField] LayerMask losBlockers;
    [SerializeField] float losRadius = 0.1f;

    void Reset()
    {
        var sc = GetComponent<SphereCollider>();
        sc.isTrigger = true; sc.radius = radius;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
    }

    void OnValidate()
    {
        var sc = GetComponent<SphereCollider>();
        if (sc) sc.radius = radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayers) == 0) return;
        var t = other.GetComponent<Targetable>();
        if (!t) return;
        if (_set.Add(t)) { 
            t.WillDisable += OnTargetDisabled;
            Debug.Log("Object entered query");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var t = other.GetComponent<Targetable>();
        if (t && _set.Remove(t)) {
            t.WillDisable -= OnTargetDisabled;
            Debug.Log("Object left query");
        }
    }
    void OnTargetDisabled(Targetable t)
    {
        if (_set.Remove(t)) t.WillDisable -= OnTargetDisabled;
    }

    public bool HasLineOfSight(Targetable t)
    {
        if(!t) return false;
        Vector3 dir = t.AimPoint.position - transform.position;
        float distance = dir.magnitude;
        if (distance <= 0f) return true;

        return !Physics.SphereCast(transform.position, losRadius, dir / distance, out _,
            distance, losBlockers, QueryTriggerInteraction.Ignore);
    }

    public TargetContext BuildContext(Vector3 origin, Vector3 forward, float maxDistance)
    {
        return new TargetContext
        {
            origin = origin,
            forward = forward,
            maxDistance = maxDistance,
            angle = halfAngleDeg,
            mask = targetLayers
        };
    }
}
