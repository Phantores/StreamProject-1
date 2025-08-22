using System.Threading;
using System.Threading.Tasks;
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
        #region MovementFields

        Vector3 _motionVector = Vector3.zero;
        Quaternion Rotation() => _controller.transform.rotation;

        float gravity = 0;

        #endregion

        #region WeaponFields

        Weapon _currentWeapon;

        bool isSemiFiring;
        bool isAutoFiring;
        bool chargeFired;
        bool firedOnce;

        Task _burstTask;
        float chargeTimeStamp;

        FireMode _fireMode => _currentWeapon.data.GameData.fireMode;
        WeaponGameData _weaponData => _currentWeapon.data.GameData;

        #endregion
        public MainState(PlayerController controller)
        {
            _controller = controller;
        }

        public void SubUpdate(float dt, PlayerContext ctx) 
        {
            Move(dt, ctx);
            HandleFire(ctx);
        }
        public void Enter(ITransition<StateEnum> via, PlayerContext ctx) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            InputManager.Instance.OnFirePressed += () => isSemiFiring = true;
            InputManager.Instance.OnHoldChanged += state => {
                if(chargeFired) chargeFired = false;
                isAutoFiring = true;
                chargeTimeStamp = Time.time;
            };
        }
        
        public void Exit(ITransition<StateEnum> via, PlayerContext ctx) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            InputManager.Instance.OnFirePressed -= () => isSemiFiring = true;
            InputManager.Instance.OnHoldChanged -= state => {
                if (chargeFired) chargeFired = false;
                isAutoFiring = true;
                chargeTimeStamp = Time.time;
            };
        }

        #region Methods
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

        void Choose(int index, PlayerContext ctx)
        {
            if (!System.Enum.IsDefined(typeof(WeaponHandler.HoldState), index)) return;
            ctx.wh.PickWeapon((WeaponHandler.HoldState)index);
            _currentWeapon = ctx.wh.GetCurrentWeapon();
        }

        void HandleFire(PlayerContext ctx)
        {
            if (_fireMode != FireMode.SemiAuto && isSemiFiring) isSemiFiring = false;
            if (_currentWeapon == null) return;

            if(_burstTask == null || _burstTask.IsCompleted) {
                if (_fireMode == FireMode.SemiAuto)
                {
                    if (isSemiFiring)
                    {
                        _burstTask = BurstCoroutine(_weaponData.burstCount,
                            _weaponData.burstRate, _weaponData.fireRate, ctx);
                    }
                } else if(_fireMode == FireMode.Auto)
                {
                    if(isAutoFiring)
                    {
                        _burstTask = BurstCoroutine(_weaponData.burstCount,
                            _weaponData.burstRate, _weaponData.fireRate, ctx);
                    }
                } else if(_fireMode == FireMode.Charged)
                {
                    if (!chargeFired && !isAutoFiring)
                    {
                        chargeFired = true;
                        _burstTask = BurstCoroutine(_weaponData.burstCount,
                            _weaponData.burstRate, _weaponData.fireRate, ctx);
                    }
                }
            }
        }

        void Shoot(PlayerContext ctx)
        {
            if (_currentWeapon.currentAmmo > 0)
            {
                ctx.wh.Shoot(Time.time - chargeTimeStamp, _currentWeapon);
            }
            else if (_currentWeapon.carriedAmmo > 0 && !firedOnce)
            {
                _currentWeapon.Reload();
            }
            else
            {
                // do stuff
            }
        }

        async Task BurstCoroutine(int amount, float time, float delay, PlayerContext ctx)
        {
            isSemiFiring = false;
            for(int i  = 0; i < amount; i++)                        //Repetively call Shoot() every `time` seconds
            {
                Shoot(ctx);
                firedOnce = true;
                await CoroutineRunner.Instance.DelayScaled(time);
            }
            await CoroutineRunner.Instance.DelayScaled(delay);      //Wait additional `delay` time
            firedOnce = false;
        }
        #endregion
    }
}
