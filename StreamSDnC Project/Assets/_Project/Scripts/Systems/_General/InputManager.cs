using Controls;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    IAStandardPlayer standardControls;

    public Vector2 Move { get; private set; }
    public Vector2 Mouse { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        standardControls = new IAStandardPlayer();
        standardControls.Enable();

        standardControls.Main.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        standardControls.Main.Move.canceled += ctx => Move = Vector2.zero;

        standardControls.Main.Mouse.performed += ctx => Mouse = ctx.ReadValue<Vector2>();
        standardControls.Main.Mouse.canceled += ctx => Mouse = Vector2.zero;
    }
}
