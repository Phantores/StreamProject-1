using System.Collections.Generic;
using UnityEngine;

namespace Player{
    public sealed class PlayerContext
    {
        public readonly Transform Transform;
        public readonly MonoBehaviour Runner;
        public PlayerData Data { get; private set; }

        //

        public PlayerContext(Transform t, MonoBehaviour runner, PlayerData data)
        {
            Transform = t;
            Runner = runner;
            Data = data;
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

        [field: SerializeField] public PlayerData data { get; private set; }

        private void Awake()
        {
            LevelManager.Instance.SetPlayer(this);
            ctx = new PlayerContext(transform, this, data);
            sm = new PlayerSM(ctx, new IState<StateEnum, PlayerContext>[]
            {
                // Add the states here: new MainState(this),
            });
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