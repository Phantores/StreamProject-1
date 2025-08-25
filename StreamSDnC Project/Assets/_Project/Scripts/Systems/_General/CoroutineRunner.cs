using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CoroutineRunner : Singleton<CoroutineRunner>
{
    // Task-based delay that still uses Unity timing
    public Task DelayScaled(float seconds, CancellationToken ct = default)
        => DelayInternal(seconds, useUnscaled: false, ct);

    public Task DelayUnscaled(float seconds, CancellationToken ct = default)
        => DelayInternal(seconds, useUnscaled: true, ct);

    Task DelayInternal(float seconds, bool useUnscaled, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>();
        var routine = DelayRoutine(seconds, useUnscaled, ct, tcs);
        var handle = StartCoroutine(routine);

        if(ct.CanBeCanceled)
        {
            ct.Register(() =>
            {
                if (handle != null) StopCoroutine(handle);
                tcs.TrySetCanceled(ct);
            });
        }

        return tcs.Task;
    }

    System.Collections.IEnumerator DelayRoutine(
        float seconds, bool unscaled, CancellationToken ct, TaskCompletionSource<bool> tcs)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (ct.IsCancellationRequested) { tcs.TrySetCanceled(); yield break; }
            elapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
        tcs.TrySetResult(true);
    }
}

