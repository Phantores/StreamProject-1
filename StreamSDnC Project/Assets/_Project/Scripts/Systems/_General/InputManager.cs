using Controls;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : Singleton<InputManager>
{
    IAStandardPlayer standardControls;

    public Vector2 Move { get; private set; }
    public Vector2 Mouse { get; private set; }

    public Vector2 MousePosition => standardControls.Main.MousePos.ReadValue<Vector2>();

    public bool Jump { get; private set; }
    public bool Run { get; private set; }
    public bool Crouch { get; private set; }

    public bool IsAiming { get; private set; }

    public bool IsHoldingFire { get; private set; }

    public event Action OnFirePressed;
    public event Action<bool> OnHoldChanged;

    public event Action<int> WeaponChanged;

    public event Action OnReloadTap;

    protected override void Awake()
    {
        base.Awake();
        standardControls = new IAStandardPlayer();
        standardControls.Enable();

        standardControls.Main.Move.performed += OnMovePerformed;
        standardControls.Main.Move.canceled += OnMoveCanceled;

        standardControls.Main.Mouse.performed += OnMousePerformed;
        standardControls.Main.Mouse.canceled += OnMouseCanceled;

        standardControls.Main.Jump.performed += OnJumpPerformed;
        standardControls.Main.Jump.canceled += OnJumpCanceled;

        standardControls.Main.Run.performed += OnRunPerformed;
        standardControls.Main.Run.canceled += OnRunCanceled;

        standardControls.Main.Crouch.performed += OnCrouchPerformed;
        standardControls.Main.Crouch.canceled += OnCrouchCanceled;

        standardControls.Firing.Aim.performed += OnAimPerformed;
        standardControls.Firing.Aim.canceled += OnAimCanceled;

        standardControls.Firing.PressFire.performed += OnPressFirePerformed;

        standardControls.Firing.HoldFire.started += HoldStart;
        standardControls.Firing.HoldFire.canceled += HoldEnd;

        standardControls.Firing.ChooseOne.performed += OnChooseOnePerformed;
        standardControls.Firing.ChooseTwo.performed += OnChooseTwoPerformed;
        standardControls.Firing.Holster.performed += OnHolsterPerformed;

        standardControls.Firing.Reload.performed += OnReloadPerformed;
    }

    public void Disable()
    {
        standardControls.Disable();
    }

    public void Enable()
    {
        standardControls.Enable();
    }

    public void OnDisable()
    {
        standardControls.Main.Move.performed -= OnMovePerformed;
        standardControls.Main.Move.canceled -= OnMoveCanceled;

        standardControls.Main.Mouse.performed -= OnMousePerformed;
        standardControls.Main.Mouse.canceled -= OnMouseCanceled;

        standardControls.Main.Jump.performed -= OnJumpPerformed;
        standardControls.Main.Jump.canceled -= OnJumpCanceled;

        standardControls.Main.Run.performed -= OnRunPerformed;
        standardControls.Main.Run.canceled -= OnRunCanceled;

        standardControls.Main.Crouch.performed -= OnCrouchPerformed;
        standardControls.Main.Crouch.canceled -= OnCrouchCanceled;

        standardControls.Firing.Aim.performed -= OnAimPerformed;
        standardControls.Firing.Aim.canceled -= OnAimCanceled;

        standardControls.Firing.PressFire.performed -= OnPressFirePerformed;

        standardControls.Firing.HoldFire.started -= HoldStart;
        standardControls.Firing.HoldFire.canceled -= HoldEnd;

        standardControls.Firing.ChooseOne.performed -= OnChooseOnePerformed;
        standardControls.Firing.ChooseTwo.performed -= OnChooseTwoPerformed;
        standardControls.Firing.Holster.performed -= OnHolsterPerformed;

        standardControls.Firing.Reload.performed -= OnReloadPerformed;
    }

    #region --- Handler Methods ---

    private void OnMovePerformed(InputAction.CallbackContext ctx) => Move = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => Move = Vector2.zero;

    private void OnMousePerformed(InputAction.CallbackContext ctx) => Mouse = ctx.ReadValue<Vector2>();
    private void OnMouseCanceled(InputAction.CallbackContext ctx) => Mouse = Vector2.zero;

    private void OnJumpPerformed(InputAction.CallbackContext ctx) => Jump = true;
    private void OnJumpCanceled(InputAction.CallbackContext ctx) => Jump = false;

    private void OnRunPerformed(InputAction.CallbackContext ctx) => Run = true;
    private void OnRunCanceled(InputAction.CallbackContext ctx) => Run = false;

    private void OnCrouchPerformed(InputAction.CallbackContext ctx) => Crouch = true;
    private void OnCrouchCanceled(InputAction.CallbackContext ctx) => Crouch = false;

    private void OnAimPerformed(InputAction.CallbackContext ctx) => IsAiming = true;
    private void OnAimCanceled(InputAction.CallbackContext ctx) => IsAiming = false;

    private void OnPressFirePerformed(InputAction.CallbackContext ctx) => TriggerAction(ctx, OnFirePressed);

    private void OnChooseOnePerformed(InputAction.CallbackContext ctx) => Choose(ctx, 1);
    private void OnChooseTwoPerformed(InputAction.CallbackContext ctx) => Choose(ctx, 2);
    private void OnHolsterPerformed(InputAction.CallbackContext ctx) => Choose(ctx, 0);

    private void OnReloadPerformed(InputAction.CallbackContext ctx) => TriggerAction(ctx, OnReloadTap);

    #endregion

    void TriggerAction(InputAction.CallbackContext ctx, Action action) { action?.Invoke(); }
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

    void Choose(InputAction.CallbackContext ctx, int num)
    {
        if(num <= 2 && num >= 0)
        {
            WeaponChanged?.Invoke(num);
        }
    }
}
