using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class QuerySensor : MonoBehaviour
{
    public readonly HashSet<Targetable> _set = new();
    private readonly HashSet<Targetable> _coneSet = new();

    PersistentQuery query;

    [SerializeField] LayerMask targetLayers;
    [SerializeField] float radius = 25f;
    [SerializeField] float halfAngleDeg = 30f;
    [SerializeField] LayerMask losBlockers;
    [SerializeField] float losRadius = 0.1f;
    [SerializeField] bool queryPersistently;

    Targetable pointed;

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

    void Update()
    {
        if (!queryPersistently) return;
        _coneSet.Clear();
        foreach (var t in _set)
        {
            if (!t || !t.AimPoint) continue;
            Vector3 toTarget = t.AimPoint.position - transform.position;

            float dot = Vector3.Dot(transform.forward, toTarget.normalized);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            if (angle <= halfAngleDeg)
            {
                _coneSet.Add(t);
            }
        }

        if (query != null)
        {
            query.UpdateCandidates(new List<Targetable>(_coneSet), BuildContext(transform.position, transform.forward, radius));
            Targetable bestTarget = query.RunQuery(BuildContext(transform.position, transform.forward, radius), _coneSet.Count);
            if (bestTarget != null)
            {
                // Use bestTarget (e.g., update widgets, highlight, etc.)
                if (bestTarget != pointed)
                {
                    Debug.Log("found a new best target");
                    bestTarget.UpdateWidgets(true);
                    if(pointed) pointed.UpdateWidgets(false);
                    pointed = bestTarget;
                }
            } else
            {
                if(pointed) pointed.UpdateWidgets(false);
                pointed = null;
            }
        }
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
        if (t && _coneSet.Remove(t)) { }
    }

    private void OnEnable()
    {
        query = new PersistentQuery(_coneSet.Count);
        query.UpdateFilters(new List<ITargetFilter>() { new AcceptFilter(), new ConeFilter() });
        query.UpdateScorers(new List<ITargetScorer>() { new ExtraScorer(), new SpikeScorer(0) });
    }
    private void OnDisable()
    {
        query?.Dispose();
        query = null;
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float angleRad = Mathf.Deg2Rad * halfAngleDeg;

        // Forward direction
        Vector3 forward = transform.forward;

        // Four directions around the cone
        Vector3[] directions = new Vector3[]
        {
            Quaternion.AngleAxis(halfAngleDeg, transform.up) * forward,
            Quaternion.AngleAxis(-halfAngleDeg, transform.up) * forward,
            Quaternion.AngleAxis(halfAngleDeg, transform.right) * forward,
            Quaternion.AngleAxis(-halfAngleDeg, transform.right) * forward
        };

        foreach (var dir in directions)
        {
            Gizmos.DrawLine(transform.position, transform.position + dir * radius);
        }
    }
}
