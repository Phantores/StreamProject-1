using UnityEngine;
using UnityEngine.UI;

namespace WorldUI{
    public class IconPresenter : Presenter
    {
        public Image Image;

        public override void ApplyData(IWorldUIData data)
        {
            if (Image == null) {
                Image = GetComponent<Image>();
                if (Image == null) Debug.LogWarning("Haven't found any image in children");
            }
            if (data is IconData d)
            {
                Image.sprite = d.sprite;
                Image.color = d.color == default ? Color.white : d.color;
                if (Rect) Rect.sizeDelta = d.size == default ? new Vector2(32, 32) : d.size;
            }
        }
    }
}
