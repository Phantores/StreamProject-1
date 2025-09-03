using UnityEngine;

namespace Player{

    public class InteractionHandler
    {
        public PlayerContext _ctx { get; private set; }
        CameraController _cameraCont => _ctx.Camera;

        public bool _isInteracting { get; private set; } = false;
        Interactable heldInteractable;

        public InteractionHandler(PlayerContext ctx) { _ctx = ctx; }

        public void CastInteract()
        {
            Debug.Log("CastInteracting");
            // 1. Check if you hold
            if(_isInteracting)
            {
                heldInteractable.Drop();
                heldInteractable = null;
                _isInteracting = false;
                return;
            }

            // 2. Cast
            Ray ray = _cameraCont.Camera.ScreenPointToRay(InputManager.Instance.MousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, _ctx.Data.interactionRange))
            {
                heldInteractable = hit.transform.gameObject.GetComponent<Interactable>();
            }

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

        public void ThrowObject()
        {
            _isInteracting = false;
        }
    }
}
