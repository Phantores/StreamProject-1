using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons{
    [RequireComponent(typeof(Animator))]
    public class ModelController : MonoBehaviour
    {
        Animator animator;

        readonly Dictionary<AnimationMode, string> animationNames = new Dictionary<AnimationMode, string>()
        {
            {AnimationMode.Idle, "Idle"},{AnimationMode.Shooting, "Shoot"},{AnimationMode.Reloading, "Reload"},{AnimationMode.Charging, "Charge"}
        };

        public enum AnimationMode
        {
            Idle, Shooting, Reloading, Charging
        }
        public AnimationMode Animode { get; private set; }


        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void ScheduleAnimation(AnimationMode anim)
        {
            Animode = anim;
            animator.Play(animationNames[anim]);
        }

        public void UpdateState(AnimationMode anim)
        {
            Animode = anim;
        }
    }
}
