using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WorldUI
{
    public class BarPresenter : Presenter
    {
        public Image ImageBG, ImageFront, ImageMid;
        public float maxUpdateVelocity;
        public float barSmoothTime;
        public float pauseTime;
        float fill;
        float vel;

        bool pause;

        public override void ApplyData(IWorldUIData data)
        {
            if (ImageBG == null)
            {
                ImageBG = GetComponent<Image>();
                if (ImageFront == null) Debug.LogWarning("Haven't found any image in children");
                if (ImageMid == null) Debug.LogWarning("Haven't found any image in children");
            }
            if (data is BarData d)
            {
                ImageBG.color = d.colorBG == default ? Color.white : d.colorBG;
                ImageFront.color = d.colorFront == default ? Color.white : d.colorFront;
                ImageMid.color = d.colorMid == default ? Color.white : d.colorMid;
                if (Rect) Rect.sizeDelta = d.size == default ? new Vector2(32, 32) : d.size;
                ImageFront.rectTransform.sizeDelta = d.size == default ? new Vector2(32, 32) : d.size;
                ImageMid.rectTransform.sizeDelta = d.size == default ? new Vector2(32, 32) : d.size;

                fill = d.fill;

                StartCoroutine(PauseFill());
            }
        }

        public override void Tick(Canvas canvas, float dt)
        {
            base.Tick(canvas, dt);
            ImageFront.fillAmount = fill;
            if(!pause) ImageMid.fillAmount = Mathf.SmoothDamp(ImageMid.fillAmount, fill, ref vel, barSmoothTime, maxUpdateVelocity);
        }

        IEnumerator PauseFill()
        {
            pause = true;
            yield return new WaitForSeconds(pauseTime);
            pause = false;
        }
    }
}
