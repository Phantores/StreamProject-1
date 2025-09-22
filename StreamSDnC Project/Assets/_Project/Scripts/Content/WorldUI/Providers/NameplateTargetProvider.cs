using UnityEngine;
using System.Collections.Generic;
using WorldUI;

public class NameplateTargetProvider : WorldUIProvider
{
    public Transform uiSocket;
    public Sprite icon;

    public override Transform WorldAnchor => uiSocket ? uiSocket : transform;

    public override IEnumerable<UIRequest> GetWorldUI()
    {
        if (icon)
            yield return new UIRequest(WidgetType.Icon, WorldAnchor,
                new IconData { sprite = icon, size = new Vector2(32, 32) }, priority: 0);
    }



    private void OnEnable()
    {
        WorldUIManager.Instance.Submit(this);
    }

    void OnDisable()
    {
        WorldUIManager.Instance.ClearAll(this);
    }

    void OnDestroy()
    {
        WorldUIManager.Instance.ClearAll(this);
    }
    public override void OnTargetedEnter() { }
    public override void OnTargetedExit() { }
}
