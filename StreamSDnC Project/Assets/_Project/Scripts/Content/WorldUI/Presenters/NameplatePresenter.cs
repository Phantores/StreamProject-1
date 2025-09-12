using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WorldUI
{
    public class NameplatePresenter : Presenter
    {
        public TextMeshProUGUI Text;
        public LayoutElement Layout;

        public override void ApplyData(IWorldUIData data)
        {
            if (!Text) Text = GetComponentInChildren<TextMeshProUGUI>(true);
            if (!Layout) Layout = GetComponent<LayoutElement>();
            if (data is NameplateData d)
            {
                Text.text = d.text ?? "";
                if (Layout) Layout.preferredWidth = d.maxWidth > 0 ? d.maxWidth : 260f;
            }
        }
    }
}
