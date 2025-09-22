using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using WorldUI;

[System.Flags]
public enum TargetType
{
    None = 0, Weapon = 1 << 0, Pickup = 1 << 1, Static = 1 << 2 // to be expanded
}

public class Targetable : MonoBehaviour
{
    [SerializeField] TargetType type;
    [SerializeField] Transform aimPoint;

    WorldUIProvider providerComponent;

    public TargetType Type => type;
    public virtual Transform AimPoint => aimPoint ? aimPoint : transform;

    public virtual bool IsAvailable() => isActiveAndEnabled;

    public virtual bool Accept(in TargetContext ctx) => true;
    public virtual float ExtraScore(in TargetContext ctx) => 0f;

    public event Action<Targetable> WillDisable;

    protected virtual void OnEnable()
    {
        providerComponent = GetComponent<WorldUIProvider>();
    }

    protected virtual void OnDisable() => WillDisable?.Invoke(this);

    public TargetableData ToStruct(TargetContext ctx)
    {
        return new TargetableData
        {
            type = Type,
            position = transform.position,
            IsAvailable = IsAvailable(),
            Accept = Accept(ctx),
            ExtraScore = ExtraScore(ctx)
        };
    }

    public void UpdateWidgets(bool targeting)
    {
        if (!providerComponent) return;
        if (targeting) providerComponent.OnTargetedEnter();
        else providerComponent.OnTargetedExit();
    }
}

public struct TargetableData
{
    public TargetType type;
    public float3 position;
    public bool IsAvailable;
    public bool Accept;
    public float ExtraScore;
}
