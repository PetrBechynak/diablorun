using DiabloLike.Combat;
using DiabloLike.World;
using UnityEngine;

namespace DiabloLike.Core
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class BossController : MonoBehaviour
    {
        private CharacterController controller;
        private Transform target;
        private Health targetHealth;
        private Health ownHealth;
        private LineRenderer laser;
        private LineRenderer dashWarning;
        private float laserAngle;
        private float nextLaserDamage;
        private float nextDashTime;
        private float nextShotTime;
        private float nextMeleeTime;
        private float moveSpeed;
        private int damageBonus;
        private bool dashing;
        private bool dashWarningActive;
        private float dashTimer;
        private Vector3 dashDirection;

        public void Configure(Transform player, int level)
        {
            target = player;
            targetHealth = player.GetComponent<Health>();
            controller = GetComponent<CharacterController>();
            ownHealth = GetComponent<Health>();
            ownHealth.DestroyOnDeath = false;
            ownHealth.Died += OnDied;
            CreateBeams();
            nextDashTime = Time.time + 3f;
            nextShotTime = Time.time + 1.5f;
            nextMeleeTime = Time.time + 1f;
            moveSpeed = 1.4f + level * 0.12f;
            damageBonus = Mathf.Max(0, level - 9) / 9 * 4;
        }

        private void Update()
        {
            if (target == null || targetHealth == null || targetHealth.IsDead || ownHealth.IsDead)
            {
                return;
            }

            UpdateLaser();
            UpdateDash();
            UpdateAttacks();
            MoveTowardPlayer();
        }

        private void MoveTowardPlayer()
        {
            var direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.magnitude > 3.2f)
            {
                controller.SimpleMove(direction.normalized * moveSpeed);
            }
        }

        private void UpdateLaser()
        {
            laserAngle += 72f * Time.deltaTime;
            var direction = Quaternion.Euler(0f, laserAngle, 0f) * Vector3.forward;
            var start = transform.position + Vector3.up * 1.35f;
            laser.SetPosition(0, start);
            laser.SetPosition(1, start + direction * 11f);

            if (Time.time < nextLaserDamage)
            {
                return;
            }

            nextLaserDamage = Time.time + 0.22f;
            var toPlayer = target.position - start;
            toPlayer.y = 0f;
            if (toPlayer.magnitude <= 11f && Vector3.Angle(direction, toPlayer.normalized) <= 8f)
            {
                targetHealth.TakeDamage(18 + damageBonus);
                EffectFactory.SpawnHit(target.position);
            }
        }

        private void UpdateAttacks()
        {
            var toPlayer = target.position - transform.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;

            if (distance <= 2.4f && Time.time >= nextMeleeTime)
            {
                nextMeleeTime = Time.time + 0.85f;
                targetHealth.TakeDamage(30 + damageBonus);
                EffectFactory.SpawnHit(target.position);
            }

            if (distance > 2.4f && distance <= 11f && Time.time >= nextShotTime)
            {
                nextShotTime = Time.time + 1.65f;
                FireProjectile(toPlayer);
            }
        }

        private void FireProjectile(Vector3 direction)
        {
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Boss Projectile";
            projectile.transform.position = transform.position + Vector3.up * 1.4f + direction.normalized * 1.05f;
            projectile.transform.localScale = Vector3.one * 0.48f;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.72f, 0.03f, 1f);
            projectile.GetComponent<Renderer>().material = material;
            var collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.9f;
            var body = projectile.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            projectile.AddComponent<EnemyProjectile>().Configure(direction, 22 + damageBonus, 9f + moveSpeed, 12f);
        }

        private void UpdateDash()
        {
            if (dashing)
            {
                dashTimer += Time.deltaTime;
                controller.Move(dashDirection * (13f * Time.deltaTime));
                if (Vector3.Distance(transform.position, target.position) < 1.2f)
                {
                    targetHealth.TakeDamage(18);
                    EffectFactory.SpawnHit(target.position);
                }

                if (dashTimer >= 0.38f)
                {
                    dashing = false;
                    dashWarning.enabled = false;
                    nextDashTime = Time.time + 4.5f;
                }

                return;
            }

            if (!dashWarningActive && Time.time >= nextDashTime)
            {
                dashDirection = target.position - transform.position;
                dashDirection.y = 0f;
                dashDirection = dashDirection.normalized;
                dashWarningActive = true;
                dashTimer = 0f;
                dashWarning.enabled = true;
                dashWarning.SetPosition(0, transform.position + Vector3.up * 0.08f);
                dashWarning.SetPosition(1, transform.position + dashDirection * 7f + Vector3.up * 0.08f);
            }
            else if (dashWarningActive && (dashTimer += Time.deltaTime) >= 0.8f)
            {
                dashWarningActive = false;
                dashing = true;
                dashTimer = 0f;
            }
        }

        private void CreateBeams()
        {
            laser = CreateBeam("Boss Rotating Laser", new Color(1f, 0.02f, 0.02f, 0.78f), 0.12f);
            dashWarning = CreateBeam("Boss Dash Warning", new Color(1f, 0.55f, 0.02f, 0.95f), 0.2f);
            dashWarning.enabled = false;
        }

        private LineRenderer CreateBeam(string name, Color color, float width)
        {
            var beamObject = new GameObject(name);
            beamObject.transform.SetParent(transform, false);
            var beam = beamObject.AddComponent<LineRenderer>();
            beam.positionCount = 2;
            beam.useWorldSpace = true;
            beam.startWidth = width;
            beam.endWidth = width * 0.35f;
            beam.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            beam.material.color = color;
            return beam;
        }

        private void OnDied(Health health)
        {
            laser.enabled = false;
            dashWarning.enabled = false;
            Destroy(gameObject, 1.2f);
        }
    }
}
