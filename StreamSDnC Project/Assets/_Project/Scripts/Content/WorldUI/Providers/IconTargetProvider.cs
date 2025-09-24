using UnityEngine;
using System.Collections.Generic;
using WorldUI;
using Audio;

public class IconTargetProvider : WorldUIProvider
{
    public Transform uiSocket;
    public Sprite icon;
    public Sprite targetIcon;

    public Audio.Event targetSound;

    public Vector2 iconSize = new Vector2(32, 32);

    bool targeted;

    Sprite toPass => targeted && targetIcon ? targetIcon : icon;

    public override Transform WorldAnchor => uiSocket ? uiSocket : transform;

    public override IEnumerable<UIRequest> GetWorldUI()
    {
        if (icon)
        {
            yield return new UIRequest(WidgetType.Icon, WorldAnchor,
                new IconData { sprite = toPass, size = iconSize }, priority: 0);
        }
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

    public override void OnTargetedEnter()
    {
        if(targetIcon)
        {
            targeted = true;
            WorldUIManager.Instance.Submit(this);
        }
    }
    public override void OnTargetedExit()
    {
        targeted = false;
        if (targetSound) AudioManager.Instance.PlayUntracked2D(targetSound);
        WorldUIManager.Instance.Submit(this);
    }
}
