using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiabloLike.Core
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6.5f;

        private Camera mainCamera;
        private CharacterController controller;
        private ProceduralActorAnimator actorAnimator;
        private Health health;
        private Mana mana;
        private WeaponDefinition weapon = WeaponCatalog.Sword;
        private LineRenderer reachLine;
        private float movementSpeedBonus;
        private int wandDamageBonus;
        private float wandRangeBonus;
        private int swordDamageBonus;
        private float swordRangeBonus;
        private bool splitWandProjectiles;
        private float projectileSizeBonus;
        private float cooldownMultiplier = 1f;
        private float nextAttackTime;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            actorAnimator = GetComponent<ProceduralActorAnimator>();
            health = GetComponent<Health>();
            mana = GetComponent<Mana>();
            mainCamera = Camera.main;
            CreateReachLine();
        }

        public void EquipWeapon(WeaponDefinition nextWeapon)
        {
            weapon = nextWeapon;
        }

        public void AddMovementSpeed(float amount)
        {
            movementSpeedBonus += Mathf.Max(0f, amount);
        }

        public void AddWandDamage(int amount)
        {
            wandDamageBonus += Mathf.Max(0, amount);
        }

        public void AddWandRange(float amount)
        {
            wandRangeBonus += Mathf.Max(0f, amount);
        }

        public void AddSwordDamage(int amount)
        {
            swordDamageBonus += Mathf.Max(0, amount);
        }

        public void AddSwordRange(float amount)
        {
            swordRangeBonus += Mathf.Max(0f, amount);
        }

        public void EnableSplitWandProjectiles()
        {
            splitWandProjectiles = true;
        }

        public void AddProjectileSize(float amount)
        {
            projectileSizeBonus += Mathf.Max(0f, amount);
        }

        public void MultiplyCooldown(float multiplier)
        {
            cooldownMultiplier *= Mathf.Clamp(multiplier, 0.35f, 1f);
        }

        private void Update()
        {
            if (health != null && health.IsDead)
            {
                return;
            }

            if (transform.position.y < -8f)
            {
                health?.TakeDamage(9999);
                return;
            }

            Move();
            FaceMouse();
            UpdateReachLine();

            if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextAttackTime)
            {
                Attack();
            }
        }

        private void Move()
        {
            var input2D = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    input2D.y += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    input2D.y -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    input2D.x += 1f;
                }

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    input2D.x -= 1f;
                }
            }

            var input = new Vector3(input2D.x, 0f, input2D.y);
            input = Vector3.ClampMagnitude(input, 1f);
            controller.SimpleMove(input * (moveSpeed + movementSpeedBonus));
        }

        private void FaceMouse()
        {
            if (mainCamera == null)
            {
                return;
            }

            if (Mouse.current == null)
            {
                return;
            }

            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            var plane = new Plane(Vector3.up, transform.position);
            if (!plane.Raycast(ray, out var distance))
            {
                return;
            }

            var target = ray.GetPoint(distance);
            var direction = target - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void Attack()
        {
            if (weapon.ManaCost > 0 && (mana == null || !mana.Spend(weapon.ManaCost)))
            {
                return;
            }

            nextAttackTime = Time.time + weapon.Cooldown * cooldownMultiplier;
            if (weapon.Id != WeaponId.Sword)
            {
                FireWandProjectile();
                return;
            }

            var center = transform.position + transform.forward * AttackCenterDistance() + Vector3.up * 0.7f;
            actorAnimator ??= GetComponent<ProceduralActorAnimator>();
            actorAnimator?.PlayAttack();
            EffectFactory.SpawnSlash(center, transform.rotation);
            var hitSomething = false;

            foreach (var hit in Physics.OverlapSphere(center, weapon.Range))
            {
                if (!hit.TryGetComponent(out CombatFaction faction) || !CanHit(faction.Faction))
                {
                    continue;
                }

                if (hit.TryGetComponent(out Health health))
                {
                    health.TakeDamage(CurrentWeaponDamage());
                    EffectFactory.SpawnHit(hit.transform.position);
                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                DiabloAudio.Play(GameSfx.SwordHit, 0.08f);
            }
        }

        private static bool CanHit(Faction faction)
        {
            return faction == Faction.Enemy || faction == Faction.Summoner;
        }

        private float AttackCenterDistance()
        {
            return weapon.Id == WeaponId.Sword ? 1.2f : Mathf.Min(2.4f, CurrentWeaponRange() * 0.62f);
        }

        private void CreateReachLine()
        {
            var lineObject = new GameObject("Weapon Reach Line");
            lineObject.transform.SetParent(transform, false);
            reachLine = lineObject.AddComponent<LineRenderer>();
            reachLine.positionCount = 2;
            reachLine.startWidth = 0.035f;
            reachLine.endWidth = 0.015f;
            reachLine.useWorldSpace = true;
            reachLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            reachLine.material.color = new Color(1f, 0.03f, 0.02f, 0.85f);
        }

        private void UpdateReachLine()
        {
            if (reachLine == null)
            {
                return;
            }

            var start = transform.position + Vector3.up * 0.08f;
            var end = start + transform.forward * CurrentWeaponRange();
            reachLine.SetPosition(0, start);
            reachLine.SetPosition(1, end);
        }

        private void FireWandProjectile()
        {
            actorAnimator ??= GetComponent<ProceduralActorAnimator>();
            actorAnimator?.PlayAttack();
            DiabloAudio.Play(GameSfx.WandCast, 0.07f);
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Wand Projectile";
            projectile.transform.position = transform.position + transform.forward * 0.85f + Vector3.up * 0.85f;
            projectile.transform.localScale = Vector3.one * (0.42f + projectileSizeBonus);
            projectile.GetComponent<Renderer>().material = CreateProjectileMaterial();
            var collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.85f + projectileSizeBonus;
            var body = projectile.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            projectile.AddComponent<WandProjectile>().Configure(transform.forward, CurrentWeaponDamage(), 10.5f, CurrentWeaponRange(), splitWandProjectiles);
        }

        private int CurrentWeaponDamage()
        {
            return weapon.Damage + (weapon.Id == WeaponId.Sword ? swordDamageBonus : wandDamageBonus);
        }

        private float CurrentWeaponRange()
        {
            return weapon.Range + (weapon.Id == WeaponId.Sword ? swordRangeBonus : wandRangeBonus);
        }

        private static Material CreateProjectileMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.2f, 0.55f, 1f);
            return material;
        }
    }
}
