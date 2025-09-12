using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class TargetQuery
{
    public static Targetable FindBest(TargetContext ctx, IEnumerable<Targetable> candidates, List<ITargetFilter> filters, List<ITargetScorer> scorers)
    {
        // 0. Convert IEnumerable to List
        var candidateList = candidates is List<Targetable> cl ? cl : new List<Targetable>(candidates);

        // 1. Data
        var candidateData = new NativeArray<TargetableData>(candidateList.Count, Allocator.TempJob);
        for (int i = 0; i < candidateList.Count; i++)
            candidateData[i] = candidateList[i].ToStruct(ctx);

        var filterArray = FilterFactory.ToNativeArray(filters, Allocator.TempJob);
        var scorerArray = ScorerFactory.ToNativeArray(scorers, Allocator.TempJob);
        var scores = new NativeArray<float>(candidateList.Count, Allocator.TempJob);
        var passes = new NativeArray<bool>(candidateList.Count, Allocator.TempJob);

        // 2. Job
        var job = new QueryJob
        {
            ctx = ctx,
            candidates = candidateData,
            filters = filterArray,
            scorers = scorerArray,
            scores = scores,
            passes = passes
        };

        var handle = job.Schedule(candidateList.Count, 32);
        handle.Complete();

        // 3. Main thread calculation
        int bestIdx = -1;
        float bestScore = float.NegativeInfinity;
        for (int i = 0; i < candidateList.Count; i++)
        {
            if (!passes[i]) continue;
            if (scores[i] > bestScore)
            {
                bestScore = scores[i];
                bestIdx = i;
            }
        }

        // 4. Cleanup
        candidateData.Dispose();
        filterArray.Dispose();
        scorerArray.Dispose();
        scores.Dispose();
        passes.Dispose();

        return bestIdx >= 0 ? candidateList[bestIdx] : null;
    }

    [BurstCompile]
    struct QueryJob : IJobParallelFor
    {
        [ReadOnly] public TargetContext ctx;
        [ReadOnly] public NativeArray<TargetableData> candidates;
        [ReadOnly] public NativeArray<FilterVariant> filters;
        [ReadOnly] public NativeArray<ScorerVariant> scorers;
        public NativeArray<float> scores;
        public NativeArray<bool> passes;

        public void Execute(int index)
        {
            var t = candidates[index];
            if (!t.IsAvailable || !t.Accept)
            {
                scores[index] = float.NegativeInfinity;
                return;
            }

            // Apply filters
            for (int i = 0; i < filters.Length; i++)
                if (!filters[i].Pass(ctx, t))
                {
                    scores[index] = float.NegativeInfinity;
                    passes[index] = false;
                    return;
                }
            passes[index] = true;

            // Apply scorers
            float score = 0f;
            for (int i = 0; i < scorers.Length; i++)
            {
                float part = scorers[i].Score(ctx, t);
                if (part < 0f)
                {
                    scores[index] = float.NegativeInfinity;
                    return;
                }
                score += part;
            }

            scores[index] = score;
        }
    }
}
