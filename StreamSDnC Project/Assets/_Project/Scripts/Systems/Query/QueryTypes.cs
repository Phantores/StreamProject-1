using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/* Instructions:
 * For each new filter/scorer do the following besides their definition:
 * 1. Add to enum
 * 2. Add to the Variant struct
 * 3. Add to the Factory class
*/

// Enums + Variants + Factories
#region General

public enum FilterType {None, Accept, Cone }
public enum ScorerType {None, Extra, Spike }

public struct FilterVariant
{
    public FilterType type;
    public AcceptFilter acceptType;
    public ConeFilter coneType;

    public bool Pass(TargetContext ctx, TargetableData t)
    {
        return type switch
        {
            FilterType.Accept => acceptType.Pass(ctx, t),
            FilterType.Cone => coneType.Pass(ctx, t),
            _ => true
        };
    }
}

public struct ScorerVariant
{
    public ScorerType type;
    public ExtraScorer extraType;
    public SpikeScorer spikeType;

    public float Score(TargetContext ctx, TargetableData t)
    {
        return type switch
        {
            ScorerType.Extra => extraType.Score(ctx, t),
            ScorerType.Spike => spikeType.Score(ctx, t),
            _ => 0f
        };
    }
}

public static class FilterFactory
{
    public static FilterVariant Wrap(ITargetFilter filter)
    {
        if (filter is AcceptFilter af)
            return new FilterVariant { type = FilterType.Accept, acceptType = af };
        if (filter is ConeFilter cf)
            return new FilterVariant {type = FilterType.Cone, coneType = cf };
        throw new ArgumentException($"Unknown filter type: {filter.GetType()}");
    }

    public static NativeArray<FilterVariant> ToNativeArray(List<ITargetFilter> filters, Allocator allocator)
    {
        var arr = new FilterVariant[filters.Count];
        for (int i = 0; i < filters.Count; i++)
            arr[i] = Wrap(filters[i]);
        return new NativeArray<FilterVariant>(arr, allocator);
    }
}

public static class ScorerFactory
{
    public static ScorerVariant Wrap(ITargetScorer scorer)
    {
        if (scorer is ExtraScorer es)
            return new ScorerVariant { type = ScorerType.Extra, extraType = es };
        if (scorer is SpikeScorer ps)
            return new ScorerVariant { type = ScorerType.Spike, spikeType = ps };
        throw new ArgumentException($"Unknown scorer type: {scorer.GetType()}");
    }

    public static NativeArray<ScorerVariant> ToNativeArray(List<ITargetScorer> scorers, Allocator allocator)
    {
        var arr = new ScorerVariant[scorers.Count];
        for (int i = 0; i < scorers.Count; i++)
            arr[i] = Wrap(scorers[i]);
        return new NativeArray<ScorerVariant>(arr, allocator);
    }
}

#endregion

// Filter implementations
#region Filters

public struct ConeFilter : ITargetFilter
{
    public bool Pass(TargetContext ctx, TargetableData t)
    {
        float3 dir = t.position - ctx.origin;
        if (Float3Utils.Magnitude(dir) > ctx.maxDistance) return false;
        float dot = Float3Utils.Dot(dir / Float3Utils.Magnitude(dir), ctx.forward);
        if (dot < Mathf.Cos(ctx.angle * Mathf.Deg2Rad)) return false;
        return true;
    }
}

public struct AcceptFilter : ITargetFilter
{
    public bool Pass(TargetContext ctx, TargetableData t)
    {
        return t.Accept;
    }
}

#endregion

// Scorer implementations
#region Scorers
public struct SpikeScorer : ITargetScorer
{
    float radialSpike, angularSpike;
    public SpikeScorer(float radialSpike = 1f, float angularSpike = 1f)
    {
        this.radialSpike = radialSpike;
        this.angularSpike = angularSpike;
    }

    public float Score(TargetContext ctx, TargetableData t)
    {
        float3 dir = t.position - ctx.origin;
        float dot = Float3Utils.Dot(dir / Float3Utils.Magnitude(dir), ctx.forward);

        float radialScore = 1 / (1 + radialSpike*Float3Utils.Magnitude(dir));
        float angularScore = 1 / (1 + angularSpike * dot);

        return radialScore * angularScore;
    }
}

public struct ExtraScorer : ITargetScorer
{
    public float Score(TargetContext ctx, TargetableData t)
    {
        return t.ExtraScore;
    }
}

#endregion