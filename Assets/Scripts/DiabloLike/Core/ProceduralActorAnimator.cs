using UnityEngine;

namespace DiabloLike.Core
{
    public sealed class ProceduralActorAnimator : MonoBehaviour
    {
        [SerializeField] private Transform modelRoot;
        [SerializeField] private Transform weapon;
        [SerializeField] private float bobAmount = 0.08f;
        [SerializeField] private float bobSpeed = 9f;

        private Vector3 lastPosition;
        private Vector3 modelStartLocalPosition;
        private Quaternion weaponStartRotation;
        private float attackPulse;
        private float hitPulse;
        private float deathProgress;
        private bool moving;
        private bool dying;
        private Animator animator;

        public void Configure(Transform nextModelRoot, Transform nextWeapon)
        {
            modelRoot = nextModelRoot;
            weapon = nextWeapon;
            animator = modelRoot != null ? modelRoot.GetComponentInChildren<Animator>() : null;
        }

        private void Start()
        {
            if (modelRoot == null)
            {
                modelRoot = transform;
            }

            lastPosition = transform.position;
            modelStartLocalPosition = modelRoot.localPosition;
            animator = modelRoot.GetComponentInChildren<Animator>();
            if (weapon != null)
            {
                weaponStartRotation = weapon.localRotation;
            }
        }

        private void Update()
        {
            var velocity = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            velocity.y = 0f;
            lastPosition = transform.position;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat("Speed", velocity.magnitude);
                var localVelocity = transform.InverseTransformDirection(velocity);
                animator.SetFloat("MoveX", Mathf.Clamp(localVelocity.x / 3f, -1f, 1f));
                animator.SetFloat("MoveY", Mathf.Clamp(localVelocity.z / 3f, -1f, 1f));
            }

            moving = moving || velocity.sqrMagnitude > 0.04f;
            var bob = moving ? Mathf.Sin(Time.time * bobSpeed) * bobAmount : Mathf.Sin(Time.time * 2.2f) * bobAmount * 0.35f;
            var hit = Mathf.MoveTowards(hitPulse, 0f, Time.deltaTime * 5f);
            hitPulse = hit;
            var walkSway = moving ? Mathf.Sin(Time.time * bobSpeed) * 4f : 0f;
            modelRoot.localPosition = modelStartLocalPosition + Vector3.up * bob;
            modelRoot.localRotation = Quaternion.Euler(0f, 0f, walkSway + hit * 5f);
            modelRoot.localScale = Vector3.one * (1f + hit * 0.04f);

            if (dying)
            {
                deathProgress = Mathf.MoveTowards(deathProgress, 1f, Time.deltaTime * 2.8f);
                modelRoot.localRotation = Quaternion.Euler(deathProgress * 88f, 0f, deathProgress * 18f);
                modelRoot.localPosition = modelStartLocalPosition + Vector3.down * deathProgress * 0.7f;
                modelRoot.localScale = Vector3.one * Mathf.Lerp(1f, 0.72f, deathProgress);
            }

            if (weapon != null)
            {
                attackPulse = Mathf.MoveTowards(attackPulse, 0f, Time.deltaTime * 7f);
                weapon.localRotation = weaponStartRotation * Quaternion.Euler(-80f * attackPulse, 22f * attackPulse, 0f);
            }

            moving = false;
        }

        private void LateUpdate()
        {
            // Animator evaluates after Update and can overwrite the death pose.
            // Reapply the corpse fall after animation evaluation so the enemy
            // visibly collapses instead of remaining upright.
            if (!dying || modelRoot == null)
            {
                return;
            }

            modelRoot.localRotation = Quaternion.Euler(deathProgress * 88f, 0f, deathProgress * 18f);
            modelRoot.localPosition = modelStartLocalPosition + Vector3.down * deathProgress * 0.7f;
            modelRoot.localScale = Vector3.one * Mathf.Lerp(1f, 0.72f, deathProgress);
        }

        public void PlayAttack()
        {
            attackPulse = 1f;
            if (animator != null && animator.runtimeAnimatorController != null && animator.parameters.Length > 0)
            {
                for (var i = 0; i < animator.parameters.Length; i++)
                {
                    if (animator.parameters[i].name == "Attack" && animator.parameters[i].type == AnimatorControllerParameterType.Trigger)
                    {
                        animator.SetTrigger("Attack");
                        break;
                    }
                }
            }
        }

        public void SetMoving(bool value)
        {
            moving = value;
        }

        public void PlayHit()
        {
            hitPulse = 1f;
            TriggerIfExists("Hit");
        }

        public void PlayDeath()
        {
            dying = true;
            deathProgress = 0f;
            // The imported death clip keeps restoring an upright pose on this
            // character setup. Disable it so the deterministic corpse fall below
            // cannot be overwritten by Animator evaluation.
            if (animator != null)
            {
                animator.enabled = false;
            }
            TriggerIfExists("Death");
        }

        private void TriggerIfExists(string triggerName)
        {
            if (animator == null && modelRoot != null)
            {
                animator = modelRoot.GetComponentInChildren<Animator>();
            }

            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == triggerName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(triggerName);
                    return;
                }
            }
        }
    }
}
