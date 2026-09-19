using DiabloLike.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace DiabloLike.World
{
    public sealed class LavaHazard : MonoBehaviour
    {
        private float nextDamageTime;
        private float[] bridgeXPositions = System.Array.Empty<float>();

        public void Configure(params float[] bridgePositions)
        {
            bridgeXPositions = bridgePositions ?? System.Array.Empty<float>();
        }

        private void Update()
        {
            if (Time.time < nextDamageTime)
            {
                return;
            }

            nextDamageTime = Time.time + 0.45f;
            var halfExtents = new Vector3(transform.lossyScale.x * 0.5f, 0.35f, transform.lossyScale.z * 0.5f);
            var colliders = Physics.OverlapBox(transform.position + Vector3.up * 0.25f, halfExtents);
            var damaged = new HashSet<Health>();
            foreach (var collider in colliders)
            {
                var health = collider.GetComponentInParent<Health>();
                if (health == null || health.IsDead || !damaged.Add(health))
                {
                    continue;
                }

                var x = health.transform.position.x;
                var onBridge = false;
                foreach (var bridgeX in bridgeXPositions)
                {
                    if (Mathf.Abs(x - bridgeX) < 0.95f)
                    {
                        onBridge = true;
                        break;
                    }
                }

                if (!onBridge)
                {
                    health.TakeDamage(12);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time < nextDamageTime || !other.CompareTag("Player"))
            {
                return;
            }

            foreach (var bridgeX in bridgeXPositions)
            {
                if (Mathf.Abs(other.transform.position.x - bridgeX) < 0.95f)
                {
                    return;
                }
            }

            var health = other.GetComponent<Health>();
            if (health == null || health.IsDead)
            {
                return;
            }

            nextDamageTime = Time.time + 0.45f;
            health.TakeDamage(12);
        }
    }
}
