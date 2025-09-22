using System.Collections.Generic;
using UnityEngine;

namespace WorldUI{
    public enum WidgetType {Icon, Nameplate, Tooltip, Bar}

    public interface IWorldUIData { }

    #region WidgetTypeStructs

    [System.Serializable] public struct IconData : IWorldUIData { 
        public Sprite sprite;
        public Color color;
        public Vector2 size;
    }
    [System.Serializable] public struct NameplateData : IWorldUIData
    {
        public string text;
        public float maxWidth;
    }
    [System.Serializable] public struct TooltipData : IWorldUIData { }
    [System.Serializable] public struct BarData : IWorldUIData
    {
        public Vector2 size;
        public float fill;
        public Color colorBG, colorFront, colorMid;
    }

    #endregion

    public struct UIRequest
    {
        public WidgetType Type;
        public Transform Anchor;
        public int Priority;
        public float Lifetime;
        public IWorldUIData Data;

        public UIRequest(WidgetType type, Transform anchor, IWorldUIData data, int priority = 0, float lifetime = 0f)
        {
            Type = type;
            Anchor = anchor;
            Priority = priority;
            Lifetime = lifetime;
            Data = data;
        }
    }

    public abstract class WorldUIProvider : MonoBehaviour
    {
        public abstract Transform WorldAnchor { get; }
        public abstract IEnumerable<UIRequest> GetWorldUI();
        public abstract void OnTargetedEnter();
        public abstract void OnTargetedExit();
    }
}
