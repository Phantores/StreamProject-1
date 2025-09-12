using System.Collections.Generic;
using UnityEngine;
using WorldUI;

public class WorldUIManager : Singleton<WorldUIManager>
{
    [Header("Canvas")]
    public Canvas Canvas;

    [Header("Prefabs")]
    public IconPresenter IconPrefab;
    public NameplatePresenter NamePlatePrefab;

    [Header("Pooling")]
    public int PrewarmPerType = 8;

    Camera _cam;
    Pool<IconPresenter> _iconPool;
    Pool<NameplatePresenter> _nameplatePool;

    readonly Dictionary<(IWorldUIProvider provider, WidgetType type), Presenter> _active = new();

    protected override void Awake()
    {
        base.Awake();
        if(!Canvas) Canvas = GetComponentInChildren<Canvas>(true);
        _cam = Camera.main;

        var parent = Canvas ? Canvas.transform : transform;
        if (IconPrefab)     _iconPool       = new Pool<IconPresenter>(IconPrefab, parent, PrewarmPerType);
        if (NamePlatePrefab)_nameplatePool  = new Pool<NameplatePresenter>(NamePlatePrefab, parent, PrewarmPerType);
    }

    public void SetCamera(Camera cam) => _cam = cam;

    private void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime;
        foreach(var kv in _active)
        {
            kv.Value.Tick(Canvas, dt);
        }
    }

    public void Submit(IWorldUIProvider provider)
    {
        if (provider == null) return;

        _seenKeys.Clear();

        foreach(var req in provider.GetWorldUI())
        {
            var key = (provider, req.Type);
            _seenKeys.Add(key);

            if(_active.TryGetValue(key, out var presenter))
            {
                presenter.Anchor = req.Anchor ? req.Anchor : provider.WorldAnchor;
                presenter.ApplyData(req.Data);
            }
            else
            {
                presenter = Spawn(req);
                _active[key] = presenter;
            }
        }

        _toRecycle.Clear();
        foreach (var kv in _active)
            if (kv.Key.provider == provider && !_seenKeys.Contains((kv.Key.provider, kv.Key.type)))
                _toRecycle.Add(kv.Key);
        foreach (var key in _toRecycle) DespawnKey(key);
    }

    public void ClearAll(IWorldUIProvider provider)
    {
        _toRecycle.Clear();
        foreach (var kv in _active) if (kv.Key.provider == provider) _toRecycle.Add(kv.Key);
        foreach(var key in _toRecycle) DespawnKey(key);
    }

    Presenter Spawn(UIRequest request)
    {
        var anchor = request.Anchor;
        if(!anchor && request.Data is null) { Debug.LogWarning("Spawn with no anchor/data"); }

        Presenter p = request.Type switch
        {
            WidgetType.Icon => _iconPool?.Spawn(),
            WidgetType.Nameplate => _nameplatePool?.Spawn(),
            _ => null
        };

        if (p == null) return null;
        p.Rect.SetParent(Canvas.transform, false);
        p.Attach(_cam, request);
        if (p.Anchor == null) p.Anchor = anchor;
        return p;
    }

    void DespawnKey((IWorldUIProvider provider, WidgetType type) key)
    {
        if(!_active.TryGetValue(key, out var p) || p == null)
        {
            _active.Remove(key);
            return;
        }

        p.OnDespawn();
        switch(key.type)
        {
            case WidgetType.Icon: _iconPool?.Despawn((IconPresenter)p); break;
            case WidgetType.Nameplate: _nameplatePool?.Despawn((NameplatePresenter)p); break;
            default: Destroy(p.gameObject); break;
        }
        _active.Remove(key);
    }

    readonly HashSet<(IWorldUIProvider, WidgetType)> _seenKeys = new();
    readonly List<(IWorldUIProvider, WidgetType)> _toRecycle = new();
}
