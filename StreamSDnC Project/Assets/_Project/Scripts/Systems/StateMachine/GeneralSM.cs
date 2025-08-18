using System.Collections.Generic;
using UnityEngine;

public interface IState<TId, TCtx> where TId : System.Enum
{
    TId Id { get; }
    public void SubUpdate(float dt, TCtx ctx);

    public void Enter(ITransition<TId> via, TCtx ctx);
    public void Exit(ITransition<TId> via, TCtx ctx);

    bool CanExit(ITransition<TId> via, TCtx ctx) => true;
    bool CanEnter(ITransition<TId> via, TCtx ctx) => true;
}

public interface ITransition<T> where T: System.Enum
{
    T From { get; }
    T To { get; }
}
public record Transition<T> (T From, T To) : ITransition<T> where T : System.Enum;

public abstract class StateMachine<TId, TCtx> where TId : System.Enum
{
    public IState<TId, TCtx> Current { get; private set; }
    protected readonly Dictionary<TId, IState<TId, TCtx>> States;
    protected virtual bool IsAllowed(TId from, TId to) => true;

    protected readonly TCtx Ctx;

    protected StateMachine(IEnumerable<IState<TId, TCtx>> states, TId initial, TCtx ctx)
    {
        Ctx = ctx;
        States = new Dictionary<TId, IState<TId, TCtx>> ();
        foreach (var s in states) States[s.Id] = s;

        Current = States[initial];
        Current.Enter(new Transition<TId>(initial, initial), Ctx);
    }

    public bool Change(ITransition<TId> t)
    {
        if (!EqualityComparer<TId>.Default.Equals(t.From, Current.Id)) return false;
        if (!IsAllowed(t.From, t.To)) return false;
        if (!Current.CanExit(t, Ctx)) return false;

        var next = States[t.To];
        if (!next.CanEnter(t, Ctx)) return false;

        Current.Exit(t, Ctx);
        Current = next;
        Current.Enter(t, Ctx);
        return true;
    }

    public void SubUpdate(float dt) => Current.SubUpdate(dt, Ctx);
}
