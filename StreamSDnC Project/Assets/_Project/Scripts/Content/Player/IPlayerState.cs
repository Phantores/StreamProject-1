using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Player.Weapons;

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
        PlayerContext _ctx;

        #region MovementFields

        Vector3 _motionVector = Vector3.zero;
        Quaternion Rotation() => _controller.transform.rotation;

        float gravity = 0;

        #endregion

        #region WeaponFields

        bool isSemiFiring;
        bool isAutoFiring;
        bool chargeFired;

        Task _burstTask;
        Task _reloadTask;

        CancellationTokenSource _reloadCts;

        float chargeTimeStamp;
        WeaponHandler.HoldState lastWeapon = WeaponHandler.HoldState.None;

        FireMode _fireMode(PlayerContext ctx) => ctx.wh.GetCurrentWeapon().data.GameData.fireMode;

        #endregion

        public MainState(PlayerController controller, PlayerContext context)
        {
            _controller = controller;
            _ctx = context;
        }

        public void SubUpdate(float dt, PlayerContext ctx) 
        {
            Move(dt);
            HandleFire();
        }
        public void Enter(ITransition<StateEnum> via, PlayerContext ctx)
        {
            _ctx = ctx;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            InputManager.Instance.OnFirePressed += SetSemiFire;
            InputManager.Instance.OnHoldChanged += OnHoldChanged;

            InputManager.Instance.WeaponChanged += OnWeaponChanged;
            InputManager.Instance.OnReloadTap += OnReloadTap;
        }
        
        public void Exit(ITransition<StateEnum> via, PlayerContext ctx) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            InputManager.Instance.OnFirePressed -= SetSemiFire;
            InputManager.Instance.OnHoldChanged -= OnHoldChanged;

            InputManager.Instance.WeaponChanged -= OnWeaponChanged;
            InputManager.Instance.OnReloadTap -= OnReloadTap;
        }
        #region Lambdas
        void OnFirePressed() => SetSemiFire();
        void OnHoldChanged(bool state) => HoldChanged(state);
        void OnWeaponChanged(int index) => Choose(index);
        void OnReloadTap() => HandleReload();

        void SetSemiFire() { isSemiFiring = true; }
        void HoldChanged(bool state)
        {
            if (_ctx.wh.Reloading) return;
            if (chargeFired) chargeFired = false;
            isAutoFiring = true;
            chargeTimeStamp = Time.time;
        }
        void Choose(int index)
        {
            _reloadCts?.Cancel(); _reloadCts.Dispose(); _reloadCts = null;
            //if (ctx.wh.Reloading) return;
            if (!System.Enum.IsDefined(typeof(WeaponHandler.HoldState), index)) return;
            _ctx.wh.PickWeapon((WeaponHandler.HoldState)index);
            lastWeapon = _ctx.wh._holdState;
        }
        #endregion

        #region Methods
        void Move(float dt)
        {
            //_controller.cc.Move(Vector3.zero); // Later velocity checks are not updated - you can yeet into a wall

            // Convert Input
            _motionVector = VectorUtils.ReadPlanar(InputManager.Instance.Move);
            _motionVector.Normalize();

            // Transform motion vector

            _motionVector = Rotation() * _motionVector * _ctx.Data.moveSpeed;

            if (InputManager.Instance.Run)
            {
                _motionVector *= _ctx.Data.runMult;
            }
            else if (InputManager.Instance.Crouch)
            {
                _motionVector *= _ctx.Data.crouchMult;
            }

            if (InputManager.Instance.Jump && _controller.cc.isGrounded)
            {
                gravity = _ctx.Data.jumpForce;
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
            _motionVector.y = Mathf.Clamp(_motionVector.y, -_ctx.Data.terminalVelocity, _ctx.Data.terminalVelocity);

            // Execute motion
            _controller.cc.Move(_motionVector * dt);
            _ctx.Camera.PassRotation(InputManager.Instance.Mouse);
        }
        void HandleFire()
        {
            if (_fireMode(_ctx) != FireMode.SemiAuto && isSemiFiring) isSemiFiring = false;
            if (_ctx.wh.GetCurrentWeapon() == null) return;

            if(_burstTask == null || _burstTask.IsCompleted && _reloadTask == null || _reloadTask.IsCompleted) {
                if (_fireMode(_ctx) == FireMode.SemiAuto)
                {
                    if (isSemiFiring)
                    {
                        isSemiFiring = false;
                        _burstTask = _ctx.wh.ShootWeapon();
                    }
                } else if(_fireMode(_ctx) == FireMode.Auto)
                {
                    if(isAutoFiring)
                    {
                        _burstTask = _ctx.wh.ShootWeapon();
                    }
                } else if(_fireMode(_ctx) == FireMode.Charged)
                {
                    if (!chargeFired && !isAutoFiring)
                    {
                        chargeFired = true;
                        _burstTask = _ctx.wh.ShootWeapon(chargeTimeStamp);
                    }
                }
            }
        }

        void HandleReload()
        {
            if(!_ctx.wh.Reloading)
            {
                _reloadTask = _ctx.wh.ReloadWeapon(_reloadCts.Token);
            } else
            {
                _reloadCts.Cancel();
            }
        }
        #endregion
    }
}
