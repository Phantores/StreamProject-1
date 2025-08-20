using Controls;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    IAStandardPlayer standardControls;

    public Vector2 Move { get; private set; }
    public Vector2 Mouse { get; private set; }

    public bool Jump { get; private set; }
    public bool Run { get; private set; }
    public bool Crouch { get; private set; }

    public bool IsHoldingFire { get; private set; }

    public event System.Action OnFirePressed;
    public event System.Action<bool> OnHoldChanged;

    protected override void Awake()
    {
        base.Awake();
        standardControls = new IAStandardPlayer();
        standardControls.Enable();

        standardControls.Main.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        standardControls.Main.Move.canceled += ctx => Move = Vector2.zero;

        standardControls.Main.Mouse.performed += ctx => Mouse = ctx.ReadValue<Vector2>();
        standardControls.Main.Mouse.canceled += ctx => Mouse = Vector2.zero;

        standardControls.Main.Jump.performed += ctx => Jump = true;
        standardControls.Main.Jump.canceled += ctx => Jump = false;

        standardControls.Main.Run.performed += ctx => Run = true;
        standardControls.Main.Run.canceled += ctx => Run = false;

        standardControls.Main.Crouch.performed += ctx => Crouch = true;
        standardControls.Main.Crouch.canceled += ctx => Crouch = false;

        standardControls.Firing.PressFire.performed += PressFire;

        standardControls.Firing.HoldFire.started += HoldStart;
        standardControls.Firing.HoldFire.canceled += HoldEnd;

    }

    public void Disable()
    {
        standardControls.Disable();
    }

    public void Enable()
    {
        standardControls.Enable();
    }

    void PressFire(InputAction.CallbackContext ctx) { OnFirePressed?.Invoke(); }
    void HoldStart(InputAction.CallbackContext ctx)
    {
        IsHoldingFire = true;
        OnHoldChanged?.Invoke(true);
    }

    void HoldEnd(InputAction.CallbackContext ctx)
    {
        IsHoldingFire = false;
        OnHoldChanged?.Invoke(false);
    }
}
