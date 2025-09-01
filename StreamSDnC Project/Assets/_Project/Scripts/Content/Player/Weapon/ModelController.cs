using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player.Weapons{
    [RequireComponent(typeof(Animator))]
    public class ModelController : MonoBehaviour
    {
        Animator animator;

        [SerializeField] string shotSpeedParam = "ShotSpeed";
        [SerializeField] string reloadSpeedParam = "ReloadSpeed";

        readonly Dictionary<AnimationMode, string> animationNames = new Dictionary<AnimationMode, string>()
        {
            {AnimationMode.Idle, "Idle"},
            {AnimationMode.Shooting, "Shoot"},
            {AnimationMode.Reloading, "Reload"},
            {AnimationMode.Charging, "Charge"}, 
            {AnimationMode.Cancel, "Cancel"}
        };

        public enum AnimationMode
        {
            Idle, Shooting, Reloading, Charging, Cancel
        }
        public AnimationMode Animode { get; private set; }


        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void ScheduleAnimation(AnimationMode anim, float animationTime = 1)
        {
            Animode = anim;
            if(anim == AnimationMode.Shooting)
            {
                float clipLength = animator.runtimeAnimatorController.animationClips.First(c => c.name == "Shoot").length;
                float speed = clipLength / animationTime;

                animator.SetFloat(shotSpeedParam, speed);
            }
            if(anim == AnimationMode.Reloading)
            {
                float clipLength = animator.runtimeAnimatorController.animationClips.First(c => c.name == "Reload").length;
                float speed = clipLength / animationTime;

                animator.SetFloat(reloadSpeedParam, speed);
            }
            animator.Play(animationNames[anim]);
        }

        public void UpdateState(AnimationMode anim)
        {
            Animode = anim;
        }
    }
}
