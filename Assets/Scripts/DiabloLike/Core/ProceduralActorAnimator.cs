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

        public void Configure(Transform nextModelRoot, Transform nextWeapon)
        {
            modelRoot = nextModelRoot;
            weapon = nextWeapon;
        }

        private void Start()
        {
            if (modelRoot == null)
            {
                modelRoot = transform;
            }

            lastPosition = transform.position;
            modelStartLocalPosition = modelRoot.localPosition;
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

            var moving = velocity.sqrMagnitude > 0.04f;
            var bob = moving ? Mathf.Sin(Time.time * bobSpeed) * bobAmount : Mathf.Sin(Time.time * 2.2f) * bobAmount * 0.35f;
            modelRoot.localPosition = modelStartLocalPosition + Vector3.up * bob;

            if (weapon != null)
            {
                attackPulse = Mathf.MoveTowards(attackPulse, 0f, Time.deltaTime * 7f);
                weapon.localRotation = weaponStartRotation * Quaternion.Euler(-80f * attackPulse, 22f * attackPulse, 0f);
            }
        }

        public void PlayAttack()
        {
            attackPulse = 1f;
        }
    }
}
