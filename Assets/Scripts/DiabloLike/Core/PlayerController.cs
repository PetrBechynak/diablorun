using System.Collections;
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
        private bool explodingWandProjectiles;
        private bool splitAfterExplosion;
        private bool hardenedBullet;
        private float projectileSizeBonus;
        private float cooldownMultiplier = 1f;
        private float nextAttackTime;
        private GameObject swordVisual;
        private GameObject wandVisual;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            actorAnimator = GetComponent<ProceduralActorAnimator>();
            health = GetComponent<Health>();
            mana = GetComponent<Mana>();
            mainCamera = Camera.main;
            CreateReachLine();
        }

        private void Start()
        {
            CreateWeaponVisuals();
        }

        public void EquipWeapon(WeaponDefinition nextWeapon)
        {
            weapon = nextWeapon;
            if (swordVisual != null)
            {
                swordVisual.SetActive(weapon.Id == WeaponId.Sword);
            }

            if (wandVisual != null)
            {
                wandVisual.SetActive(weapon.Id != WeaponId.Sword);
            }
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

        public void EnableExplodingWandProjectiles()
        {
            explodingWandProjectiles = true;
        }

        public void EnableHardenedBullet()
        {
            hardenedBullet = true;
        }

        public void SetSplitAfterExplosion(bool value)
        {
            splitAfterExplosion = value;
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
            UpdateReachLine();

            if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextAttackTime)
            {
                FaceMouseForAttack();
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
            if (input.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(input);
            }

            controller.SimpleMove(input * (moveSpeed + movementSpeedBonus));
        }

        private void FaceMouseForAttack()
        {
            if (mainCamera == null || Mouse.current == null)
            {
                return;
            }

            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            var plane = new Plane(Vector3.up, transform.position);
            if (!plane.Raycast(ray, out var distance))
            {
                return;
            }

            var direction = ray.GetPoint(distance) - transform.position;
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
                    if (health.IsDead && hit.name.StartsWith("Vlkodlak") && Vector3.Distance(transform.position, hit.transform.position) <= 2.4f)
                    {
                        StartCoroutine(SinkEnemy(hit.transform));
                    }
                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                DiabloAudio.Play(GameSfx.SwordHit, 0.08f);
            }
        }

        // Used only by isolated automated combat tests; this follows the exact
        // same attack path as the player's normal input.
        public void TriggerAttackForTest()
        {
            Attack();
        }

        private static bool CanHit(Faction faction)
        {
            return faction == Faction.Enemy || faction == Faction.Summoner;
        }

        private IEnumerator SinkEnemy(Transform enemy)
        {
            var start = enemy.position;
            var end = start + Vector3.down * 1.8f;
            var elapsed = 0f;
            while (enemy != null && elapsed < 0.45f)
            {
                elapsed += Time.deltaTime;
                enemy.position = Vector3.Lerp(start, end, elapsed / 0.45f);
                yield return null;
            }

            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
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

        private void CreateWeaponVisuals()
        {
            swordVisual = new GameObject("Visible Sword");
            swordVisual.transform.SetParent(FindRightHand() ?? transform, false);
            swordVisual.transform.localPosition = Vector3.zero;
            swordVisual.transform.localRotation = Quaternion.identity;
            swordVisual.transform.localScale = Vector3.one;

            wandVisual = new GameObject("Visible Ember Wand");
            wandVisual.transform.SetParent(FindRightHand() ?? transform, false);
            wandVisual.transform.localPosition = new Vector3(0.04f, -0.04f, 0.08f);
            wandVisual.transform.localRotation = Quaternion.Euler(-12f, 0f, -12f);
            wandVisual.transform.localScale = Vector3.one * 0.58f;
            var staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            staff.name = "Wand Staff";
            staff.transform.SetParent(wandVisual.transform, false);
            staff.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            staff.transform.localScale = new Vector3(0.07f, 0.7f, 0.07f);
            staff.GetComponent<Renderer>().material = CreateWeaponMaterial(new Color(0.3f, 0.12f, 0.04f));
            Object.Destroy(staff.GetComponent<Collider>());
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystal.name = "Wand Crystal";
            crystal.transform.SetParent(wandVisual.transform, false);
            crystal.transform.localPosition = new Vector3(0f, 1.32f, 0f);
            crystal.transform.localScale = Vector3.one * 0.22f;
            crystal.GetComponent<Renderer>().material = CreateWeaponMaterial(new Color(1f, 0.35f, 0.04f));
            Object.Destroy(crystal.GetComponent<Collider>());

            EquipWeapon(weapon);
        }

        private Transform FindRightHand()
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                var exact = child.name.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
                if (exact == "handr" || exact == "righthand")
                {
                    return child;
                }
            }
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                var name = child.name.ToLowerInvariant().Replace(" ", "").Replace("-", "");
                if (!name.Contains("ik") && (name.Contains("righthand") || name.Contains("hand_r") || name.Contains("hand.r")))
                {
                    return child;
                }
            }

            return null;
        }

        private static Material CreateWeaponMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
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
            var direction = transform.forward;
            SpawnWandProjectile(direction, CurrentWeaponDamage(), 1f);

            if (splitWandProjectiles && !splitAfterExplosion)
            {
                SpawnWandProjectile(Quaternion.Euler(0f, -18f, 0f) * direction, CurrentWeaponDamage(), 0.58f);
                SpawnWandProjectile(Quaternion.Euler(0f, 18f, 0f) * direction, CurrentWeaponDamage(), 0.58f);
            }
        }

        private void SpawnWandProjectile(Vector3 direction, int baseDamage, float damageMultiplier)
        {
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Wand Projectile";
            projectile.transform.position = transform.position + direction.normalized * 0.85f + Vector3.up * 0.85f;
            projectile.transform.localScale = Vector3.one * (hardenedBullet ? 0.46f : 0.42f) + Vector3.one * projectileSizeBonus;
            projectile.GetComponent<Renderer>().material = CreateProjectileMaterial(hardenedBullet);
            var collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.85f + projectileSizeBonus;
            var body = projectile.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            var damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageMultiplier * (hardenedBullet ? 1.2f : 1f)));
            projectile.AddComponent<WandProjectile>().Configure(direction, damage, 10.5f, CurrentWeaponRange(), false, false, explodingWandProjectiles, splitAfterExplosion && damageMultiplier == 1f);
        }

        private int CurrentWeaponDamage()
        {
            return weapon.Damage + (weapon.Id == WeaponId.Sword ? swordDamageBonus : wandDamageBonus);
        }

        private float CurrentWeaponRange()
        {
            return weapon.Range + (weapon.Id == WeaponId.Sword ? swordRangeBonus : wandRangeBonus);
        }

        private static Material CreateProjectileMaterial(bool hardened)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = hardened ? new Color(1f, 0.68f, 0.05f) : new Color(0.2f, 0.55f, 1f);
            return material;
        }
    }
}
