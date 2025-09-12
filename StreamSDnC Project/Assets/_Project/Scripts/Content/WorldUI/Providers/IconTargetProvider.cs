using UnityEngine;
using System.Collections.Generic;
using WorldUI;

public class IconTargetProvider : MonoBehaviour, IWorldUIProvider
{
    public Transform uiSocket;
    public Sprite icon;

    public Transform WorldAnchor => uiSocket ? uiSocket : transform;

    public IEnumerable<UIRequest> GetWorldUI()
    {
        if(icon)
            yield return new UIRequest(WidgetType.Icon, WorldAnchor,
                new IconData {sprite = icon, size = new Vector2(32, 32)}, priority: 0);
    }
}
