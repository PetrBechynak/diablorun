using DiabloLike.AI;
using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.UI;
using DiabloLike.World;
using UnityEngine;

namespace DiabloLike.Core
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private float speed = 3.2f;
        [SerializeField] private float attackRange = 1.45f;
        [SerializeField] private float attackCooldown = 0.9f;
        [SerializeField] private int damage = 8;
        [SerializeField] private bool ranged;
        [SerializeField] private float rangedAttackRange = 6f;
        [SerializeField] private float keepAwayRange = 3.4f;

        private CharacterController controller;
        private Transform target;
        private Health targetHealth;
        private Health ownHealth;
        private GameDirector gameDirector;
        private ProceduralActorAnimator actorAnimator;
        private EnemySpeechBubble speechBubble;
        private float nextAttackTime;
        private float nextBarkTime;
        private float nextSearchBarkTime;
        private Vector3 aiMoveTarget;
        private float nextAiRefresh;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            actorAnimator = GetComponent<ProceduralActorAnimator>();
            ownHealth = GetComponent<Health>();
            speechBubble = GetComponent<EnemySpeechBubble>();
            if (ownHealth != null)
            {
                ownHealth.DestroyOnDeath = false;
                ownHealth.Damaged += OnDamaged;
                ownHealth.Died += OnDied;
            }
        }

        public void Configure(Transform player, GameDirector director)
        {
            target = player;
            gameDirector = director;
            targetHealth = player.GetComponent<Health>();
            TryBark(EnemyBarkEvent.Spawn, 0.42f, true);
        }

        public void ConfigureRanged()
        {
            ranged = true;
            speed = 2.7f;
            damage = 6;
            attackCooldown = 1.35f;
        }

        private void Update()
        {
            if (target == null || targetHealth == null || targetHealth.IsDead)
            {
                return;
            }

            RefreshAiTarget();
            var toPlayer = target.position - transform.position;
            var toAiTarget = aiMoveTarget - transform.position;
            var activeAttackRange = ranged ? rangedAttackRange : attackRange;
            var moveDirection = ChooseMoveDirection(toPlayer, toAiTarget, activeAttackRange);
            controller.SimpleMove(moveDirection * speed);

            if (Time.time >= nextSearchBarkTime && toPlayer.magnitude > attackRange * 1.5f)
            {
                nextSearchBarkTime = Time.time + Random.Range(4.5f, 8.5f);
                TryBark(EnemyBarkEvent.Searching, 0.16f);
            }

            var facing = toPlayer;
            facing.y = 0f;
            if (facing.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(facing);
            }

            if (toPlayer.magnitude <= activeAttackRange && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                actorAnimator ??= GetComponent<ProceduralActorAnimator>(); 
                actorAnimator?.PlayAttack();
                if (ranged)
                {
                    FireProjectile(toPlayer);
                }
                else
                {
                    targetHealth.TakeDamage(damage);
                    EffectFactory.SpawnHit(target.position);
                }

                TryBark(EnemyBarkEvent.HitPlayer, 0.38f);
                DiabloAudio.Play(GameSfx.SwordHit, 0.08f);
            }
        }

        private Vector3 ChooseMoveDirection(Vector3 toPlayer, Vector3 toAiTarget, float activeAttackRange)
        {
            if (!ranged)
            {
                return toPlayer.magnitude <= activeAttackRange ? Vector3.zero : toAiTarget.normalized;
            }

            if (toPlayer.magnitude < keepAwayRange)
            {
                return -toPlayer.normalized;
            }

            return toPlayer.magnitude <= activeAttackRange ? Vector3.zero : toAiTarget.normalized;
        }

        private void FireProjectile(Vector3 toPlayer)
        {
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Enemy Projectile";
            projectile.transform.position = transform.position + Vector3.up * 1.1f + toPlayer.normalized * 0.65f;
            projectile.transform.localScale = Vector3.one * 0.34f;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(1f, 0.16f, 0.05f);
            projectile.GetComponent<Renderer>().material = material;
            var collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.8f;
            var body = projectile.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            projectile.AddComponent<EnemyProjectile>().Configure(toPlayer, damage, 7.5f, rangedAttackRange + 1.5f);
        }

        private void RefreshAiTarget()
        {
            if (Time.time < nextAiRefresh || gameDirector == null)
            {
                return;
            }

            nextAiRefresh = Time.time + Random.Range(0.6f, 1.1f);
            var response = gameDirector.AI.Ask(new AIRequest(AIFeature.NpcBehavior, name, "", gameDirector.CreateSnapshot()));
            aiMoveTarget = response.TargetPosition == default ? target.position : response.TargetPosition;
        }

        private void OnDamaged(Health damagedHealth, int amount)
        {
            if (damagedHealth.IsDead)
            {
                return;
            }

            TryBark(EnemyBarkEvent.Damaged, 0.55f);
            DiabloAudio.Play(GameSfx.EnemyHurt, 0.08f);
        }

        private void OnDied(Health deadHealth)
        {
            TryBark(EnemyBarkEvent.Dying, 0.75f, true);
            DiabloAudio.Play(GameSfx.EnemyDeath, 0.06f);
            EffectFactory.SpawnEnemyDeath(transform.position);
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            if (controller != null)
            {
                controller.enabled = false;
            }

            Destroy(gameObject, 2.8f);
        }

        private void TryBark(EnemyBarkEvent barkEvent, float chance, bool ignoreCooldown = false)
        {
            if (speechBubble == null)
            {
                speechBubble = GetComponent<EnemySpeechBubble>();
            }

            if (speechBubble == null)
            {
                return;
            }

            if (!ignoreCooldown && Time.time < nextBarkTime)
            {
                return;
            }

            if (!ignoreCooldown && Random.value > chance)
            {
                return;
            }

            nextBarkTime = Time.time + Random.Range(2.4f, 4.2f);
            speechBubble.Say(EnemyBarkLibrary.Get(barkEvent));
        }
    }
}
