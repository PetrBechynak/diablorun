using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.World;
using UnityEngine;

namespace DiabloLike.Core
{
    public sealed class WandProjectile : MonoBehaviour
    {
        private int damage;
        private float speed;
        private float maxDistance;
        private bool splitsOnHit;
        private bool explodesOnHit;
        private bool splitsAfterExplosion;
        private bool isSplitChild;
        private Vector3 startPosition;
        private Vector3 direction;

        public void Configure(Vector3 moveDirection, int projectileDamage, float projectileSpeed, float range, bool canSplit = false, bool splitChild = false, bool canExplode = false, bool splitAfterExplosion = false)
        {
            direction = moveDirection.normalized;
            damage = projectileDamage;
            speed = projectileSpeed;
            maxDistance = range;
            splitsOnHit = canSplit;
            isSplitChild = splitChild;
            explodesOnHit = canExplode;
            splitsAfterExplosion = splitAfterExplosion;
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
            if (!other.TryGetComponent(out CombatFaction faction) || (faction.Faction != Faction.Enemy && faction.Faction != Faction.Summoner))
            {
                return;
            }

            if (other.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
                EffectFactory.SpawnHit(other.transform.position);
                DiabloAudio.Play(GameSfx.WandHit, 0.08f);
            }

            if (splitsOnHit && !isSplitChild)
            {
                SpawnSplitProjectile(Quaternion.Euler(0f, -24f, 0f) * direction);
                SpawnSplitProjectile(Quaternion.Euler(0f, 24f, 0f) * direction);
            }

            if (explodesOnHit)
            {
                Explode();
            }

            if (splitsAfterExplosion && !isSplitChild)
            {
                SpawnSplitProjectile(Quaternion.Euler(0f, -18f, 0f) * direction);
                SpawnSplitProjectile(Quaternion.Euler(0f, 18f, 0f) * direction);
            }

            Destroy(gameObject);
        }

        private void Explode()
        {
            const float radius = 1.8f;
            EffectFactory.SpawnExplosion(transform.position);
            foreach (var hit in Physics.OverlapSphere(transform.position, radius))
            {
                if (hit.TryGetComponent(out CombatFaction faction)
                    && (faction.Faction == Faction.Enemy || faction.Faction == Faction.Summoner)
                    && hit.TryGetComponent(out Health health))
                {
                    health.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(damage * 0.65f)));
                }
            }
        }

        private void SpawnSplitProjectile(Vector3 splitDirection)
        {
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Wand Projectile";
            projectile.transform.position = transform.position + splitDirection.normalized * 0.25f;
            projectile.transform.localScale = Vector3.one * 0.34f;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.78f, 0.22f, 1f);
            projectile.GetComponent<Renderer>().material = material;
            var collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.85f;
            var body = projectile.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            projectile.AddComponent<WandProjectile>().Configure(splitDirection, Mathf.Max(1, Mathf.RoundToInt(damage * 0.55f)), speed * 0.9f, maxDistance * 0.45f, false, true);
        }
    }
}
