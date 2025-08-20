using UnityEngine;

namespace Player{
public enum StateEnum
{
    Off = 0, Main = 1, Falling = 2
} 

    public class OffState : IState<StateEnum, PlayerContext>
    {
        public StateEnum Id => StateEnum.Off;

        PlayerController _controller;

        public OffState(PlayerController controller)
        {
            _controller = controller;
        }

        public void SubUpdate(float dt, PlayerContext ctx) { }

        public void Enter(ITransition<StateEnum> via, PlayerContext ctx) 
        {
            InputManager.Instance.Disable();
        }
        public void Exit(ITransition<StateEnum> via, PlayerContext ctx) 
        {
            InputManager.Instance.Enable();
        }
    }

    public class MainState : IState<StateEnum, PlayerContext>
    {
        public StateEnum Id => StateEnum.Main;

        PlayerController _controller;
        Weapon _currentWeapon;

        Vector3 _motionVector = Vector3.zero;
        Quaternion Rotation() => _controller.transform.rotation;

        float gravity = 0;

        public MainState(PlayerController controller)
        {
            _controller = controller;
        }

        public void SubUpdate(float dt, PlayerContext ctx) 
        {
            Move(dt, ctx);
        }
        public void Enter(ITransition<StateEnum> via, PlayerContext ctx) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            InputManager.Instance.OnFirePressed += PressShoot;
        }
        
        public void Exit(ITransition<StateEnum> via, PlayerContext ctx) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            InputManager.Instance.OnFirePressed -= PressShoot;
        }

        void Move(float dt, PlayerContext ctx)
        {
            //_controller.cc.Move(Vector3.zero); // Later velocity checks are not updated - you can yeet into a wall

            // Convert Input
            _motionVector = VectorUtils.ReadPlanar(InputManager.Instance.Move);
            _motionVector.Normalize();

            // Transform motion vector

            _motionVector = Rotation() * _motionVector * ctx.Data.moveSpeed;

            if (InputManager.Instance.Run)
            {
                _motionVector *= ctx.Data.runMult;
            }
            else if (InputManager.Instance.Crouch)
            {
                _motionVector *= ctx.Data.crouchMult;
            }

            if (InputManager.Instance.Jump && _controller.cc.isGrounded)
            {
                gravity = ctx.Data.jumpForce;
            }
            else if (_controller.cc.isGrounded)
            {
                gravity = -0.05f;
            }
            else
            {
                gravity += Physics.gravity.y * dt;
            }

            _motionVector.y = gravity;
            _motionVector.y = Mathf.Clamp(_motionVector.y, -ctx.Data.terminalVelocity, ctx.Data.terminalVelocity);

            // Execute motion
            _controller.cc.Move(_motionVector * dt);
            ctx.Camera.PassRotation(InputManager.Instance.Mouse);
        }
        void Shoot(PlayerContext ctx)
        {
            _currentWeapon = ctx.wh.GetCurrentWeapon();
            if (_currentWeapon == null) return;

            if(_currentWeapon.data.GameData.fireMode == FireMode.SemiAuto)
            {
                
            }
        }

        void PressShoot()
        {
            if (_currentWeapon.currentAmmo > 0)
            {
                // do shoot logic
            }
            else if (_currentWeapon.carriedAmmo > 0)
            {
                _currentWeapon.Reload();
            }
            else
            {
                // do stuff
            }
        }
    }
}
