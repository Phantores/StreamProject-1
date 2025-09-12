using UnityEngine;

namespace WorldUI{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Presenter : MonoBehaviour
    {
        [Header("Common")]
        public RectTransform Rect;
        public CanvasGroup Group;
        public Transform Anchor;
        public Vector3 WorldOffset = Vector3.zero;
        public float SmoothTime = 0.08f;
        public bool OcclusionRaycast = true;
        public LayerMask Occluders;

        protected Camera _cam;
        protected Vector2 _vel;
        protected Vector2 _targetAnchored;

        public virtual void Attach(Camera cam, UIRequest request)
        {
            if(!Rect) Rect = transform as RectTransform;
            if(!Group) Group = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _cam = cam;
            Anchor = request.Anchor;
            ApplyData(request.Data);
            Group.alpha = 0f;
        }

        public abstract void ApplyData(IWorldUIData data);

        public virtual void Tick(Canvas canvas, float dt)
        {
            if (!_cam || !Anchor) return;

            Vector3 wpos = Anchor.position + WorldOffset;
            Vector3 sp = _cam.WorldToScreenPoint(wpos);
            
            bool behind = sp.z < 0f;
            bool blocked = false;
            if (!behind && OcclusionRaycast && Occluders.value != 0)
            {
                Vector3 dir = wpos - _cam.transform.position;
                blocked = Physics.Raycast(_cam.transform.position, dir.normalized, dir.magnitude, Occluders, QueryTriggerInteraction.Ignore);
            }

            bool visible = !behind && !blocked &&
                           sp.x >= 0 && sp.x <= Screen.width &&
                           sp.y >= 0 && sp.y <= Screen.height;

            var canvasRect = (RectTransform)canvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, sp,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _cam,
                out var local);

            _targetAnchored = new Vector2(Mathf.Round(local.x), Mathf.Round(local.y));
            Rect.anchoredPosition = Vector2.SmoothDamp(Rect.anchoredPosition, _targetAnchored, ref _vel, SmoothTime, Mathf.Infinity, dt);

            float targetAlpha = visible ? 1f : 0f;
            Group.alpha = Mathf.MoveTowards(Group.alpha, targetAlpha, 12f * dt);
            Group.blocksRaycasts = visible;
            Group.interactable = visible;
        }

        public virtual void OnDespawn() { }
    }
}
