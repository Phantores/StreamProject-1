using UnityEngine;
using System.Collections.Generic;
using WorldUI;

public class BarTargetProvider : WorldUIProvider
{
    public Transform uiSocket;

    public Color BGColor, MidColor, FrontColor;
    public Vector2 size = new Vector3(32,32);
    public float fill;

    HealthComponent hc;

    public override Transform WorldAnchor => uiSocket ? uiSocket : transform;

    public override IEnumerable<UIRequest> GetWorldUI()
    {
            yield return new UIRequest(WidgetType.Icon, WorldAnchor,
                new BarData {size = size, fill = fill, colorBG = BGColor, colorFront = FrontColor, colorMid = MidColor},
                priority: 0);
    }

    void UpdateFill(float Fill)
    {
        fill = Fill;
        WorldUIManager.Instance.Submit(this);
    }

    private void OnEnable()
    {
        hc = GetComponent<HealthComponent>();
        if(hc) hc.onHealthChanged += UpdateFill;
        WorldUIManager.Instance.Submit(this);
    }

    void OnDisable()
    {
        if(hc) hc.onHealthChanged -= UpdateFill;
        WorldUIManager.Instance.ClearAll(this);
    }

    void OnDestroy()
    {
        WorldUIManager.Instance.ClearAll(this);
    }

    public override void OnTargetedEnter() { }
    public override void OnTargetedExit() { }
}

