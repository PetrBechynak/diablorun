using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.World;
using UnityEngine;

namespace DiabloLike.Core
{
    public sealed class EnemyProjectile : MonoBehaviour
    {
        private int damage;
        private float speed;
        private float maxDistance;
        private Vector3 startPosition;
        private Vector3 direction;

        public void Configure(Vector3 moveDirection, int projectileDamage, float projectileSpeed, float range)
        {
            direction = moveDirection.normalized;
            damage = projectileDamage;
            speed = projectileSpeed;
            maxDistance = range;
            startPosition = transform.position;
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
            if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CombatFaction faction) || faction.Faction != Faction.Player)
            {
                return;
            }

            if (other.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
                EffectFactory.SpawnHit(other.transform.position);
                DiabloAudio.Play(GameSfx.WandHit, 0.08f);
            }

            Destroy(gameObject);
        }
    }
}
