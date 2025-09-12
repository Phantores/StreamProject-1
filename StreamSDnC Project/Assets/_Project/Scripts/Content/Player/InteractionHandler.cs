using UnityEngine;
using System.Collections.Generic;

namespace Player{

    public class InteractionHandler
    {
        public PlayerContext _ctx { get; private set; }
        CameraController _cameraCont => _ctx.Camera;

        public bool _isInteracting { get; private set; } = false;
        Interactable heldInteractable;

        ITargetFilter _filter;
        ITargetScorer _scorer;

        public InteractionHandler(PlayerContext ctx)
        {
            _ctx = ctx;
            _filter = new ConeFilter();
            _scorer = new SpikeScorer();
        }

        public void CastInteract()
        {
            Debug.Log("CastInteracting");
            // 1. Check if you hold
            if(_isInteracting)
            {
                if (heldInteractable is Interactable_Pickup ict) ict.Drop();
                heldInteractable = null;
                _isInteracting = false;
                return;
            }

            // 2. Cast
            TryTargetQuery();

            // 3. Evaluate
            if (heldInteractable != null)
            {
                switch (heldInteractable)
                {
                    case Interactable_Static ict:{
                            ict.Interact();
                            heldInteractable = null;
                            break;
                        }
                    case Interactable_Item ict:
                        {
                            ict.Interact();
                            heldInteractable = null;
                            break;
                        }
                    case Interactable_Weapon ict:
                        {
                            _ctx.wh.CollectWeapon(ict.Weapon);
                            ict.Interact();
                            heldInteractable = null;
                            break;
                        }
                    case Interactable_Pickup ict:
                        {
                            ict.SetOwner(this);
                            ict.Interact();
                            _isInteracting = true;
                            break;
                        }
                    case null:
                        {
                            Debug.LogWarning("Tried to interact with null Interactable");
                            break;
                        }
                    default:
                        {
                            heldInteractable = null;
                            Debug.LogError("Tried to intract with unknown Interactable");
                            break;
                        }
                }
            }
        }

        public void ForceDrop()
        {
            _isInteracting = false;
        }

        public void ThrowObject()
        {
            _isInteracting = false;
            if (heldInteractable is Interactable_Pickup ict) ict.ThrowObject();
        }

        public void TryTargetQuery()
        {
            Vector3 origin = _ctx.Camera.transform.position;
            Vector3 forward = _ctx.Camera.transform.forward;

            TargetContext ctx = _ctx.QuerySensor.BuildContext(origin, forward, _ctx.Data.interactionRange);

            var candidates = _ctx.QuerySensor._set;
            var filters = new List<ITargetFilter>() {_filter}; // Populate as needed
            var scorers = new List<ITargetScorer>() {_scorer}; // Populate as needed

            Targetable best = TargetQuery.FindBest(ctx, candidates, filters, scorers);

            if(best)
            {
                Debug.Log("Found best candidate");
                heldInteractable = best.gameObject.GetComponent<Interactable>();
            }
            if (heldInteractable != null && heldInteractable.needsLineOfSight)
            {
                Debug.Log("Found interactable before LOS check");
                bool check = _ctx.QuerySensor.HasLineOfSight(best);
                if (!check) heldInteractable = null;
            }
        }
    }
}
