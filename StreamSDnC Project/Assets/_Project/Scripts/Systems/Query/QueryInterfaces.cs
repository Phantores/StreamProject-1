using UnityEngine;
using Unity.Mathematics;

public interface ITargetFilter
{
    bool Pass(TargetContext ctx, TargetableData t);
}
public interface ITargetScorer
{
    float Score(TargetContext ctx, TargetableData t);
}

public struct TargetContext
{
    public float3 origin, forward;
    public float maxDistance, angle; // half angle
    public LayerMask mask;
}
