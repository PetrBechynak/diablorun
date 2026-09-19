using System;
using UnityEngine;

namespace DiabloLike.Combat
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int armor;

        public int Current => currentHealth;
        public int Max => maxHealth;
        public int Armor => armor;
        public bool IsDead => currentHealth <= 0;
        public event Action<Health, int> Damaged;
        public event Action<Health> Died;
        public bool DestroyOnDeath { get; set; } = true;

        public void Configure(int max)
        {
            maxHealth = Mathf.Max(1, max);
            currentHealth = maxHealth;
        }

        public void IncreaseMax(int amount)
        {
            amount = Mathf.Max(0, amount);
            maxHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public void IncreaseArmor(int amount)
        {
            armor += Mathf.Max(0, amount);
        }

        public void SetCurrent(int value)
        {
            currentHealth = Mathf.Clamp(value, 1, maxHealth);
        }

        public void Heal(int amount)
        {
            if (IsDead)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
        }

        public void TakeDamage(int amount)
        {
            if (IsDead)
            {
                return;
            }

            amount = Mathf.Max(0, amount - armor);
            currentHealth = Mathf.Max(0, currentHealth - amount);
            Damaged?.Invoke(this, amount);
            if (IsDead)
            {
                Died?.Invoke(this);
                if (DestroyOnDeath)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
