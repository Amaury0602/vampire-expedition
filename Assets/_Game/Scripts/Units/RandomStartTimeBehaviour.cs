using UnityEngine;

namespace Units
{
    public class RandomStartTimeBehaviour : StateMachineBehaviour
    {
        private int _lastRandomizedFrame = -1;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Time.frameCount - _lastRandomizedFrame <= 1)
            {
                return;
            }

            _lastRandomizedFrame = Time.frameCount;
            animator.Play(stateInfo.fullPathHash, layerIndex, Random.value);
        }
    }
}
