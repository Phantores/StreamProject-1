using System.Collections.Generic;
using UnityEngine;
using Player.Weapons;

namespace Player{
    public sealed class PlayerContext
    {
        public readonly Transform Transform;
        public readonly MonoBehaviour Runner;
        public readonly CameraController Camera;
        public readonly WeaponCenter WeaponCenter;
        public PlayerData Data { get; private set; }

        public WeaponHandler wh { get; private set; } = null;

        //

        public PlayerContext(Transform t, MonoBehaviour runner, PlayerData data, CameraController camera, WeaponCenter weaponCenter)
        {
            Transform = t;
            Runner = runner;
            Data = data;
            Camera = camera;

            wh = new WeaponHandler(camera, weaponCenter);
            WeaponCenter = weaponCenter;
        }

        public void UpdateData(PlayerData data)
        {
            Data = data;
        }
    }

    public sealed class PlayerSM : StateMachine<StateEnum, PlayerContext>
    {
        readonly HashSet<(StateEnum, StateEnum)> _allowed = new()
        {
            (StateEnum.Off, StateEnum.Main), (StateEnum.Main, StateEnum.Off),
            (StateEnum.Falling, StateEnum.Main), (StateEnum.Main, StateEnum.Falling)
        };

        protected override bool IsAllowed(StateEnum from, StateEnum to) => _allowed.Contains((from, to));
        public PlayerSM(PlayerContext ctx, IEnumerable<IState<StateEnum, PlayerContext>> states) : base(states, StateEnum.Main, ctx) { }
    }

    public class PlayerController : MonoBehaviour
    {
        PlayerSM sm;
        PlayerContext ctx;

        [SerializeField] CameraController PlayerCamera;
        [SerializeField] WeaponCenter WeaponCenter;

        [field: SerializeField] public PlayerData data { get; private set; }

        public CharacterController cc { get; private set; }

        private void Awake()
        {
            if(PlayerCamera == null)
            {
                PlayerCamera = Camera.main.GetComponent<CameraController>();
                if (PlayerCamera == null)
                {
                    Debug.LogError("No CameraController found on the main camera.");
                }
            } else
            {
                PlayerCamera.SetData(data.mouseSensitivity, data.viewLimit, this.gameObject);
            }
            cc = GetComponent<CharacterController>();
            ctx = new PlayerContext(transform, this, data, PlayerCamera, WeaponCenter);
            sm = new PlayerSM(ctx, new IState<StateEnum, PlayerContext>[]
            {
                new OffState(this), new MainState(this, this.ctx),
            });
        }

        private void Start()
        {
            LevelManager.Instance.SetPlayer(this);
        }

        private void Update()
        {
            sm.SubUpdate(Time.deltaTime);
        }

        public void ChangeData(PlayerData data) { 
            this.data = data; 
            ctx.UpdateData(data);
        }
    }
}