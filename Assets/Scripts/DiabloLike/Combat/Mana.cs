using UnityEngine;

namespace DiabloLike.Combat
{
    public sealed class Mana : MonoBehaviour
    {
        [SerializeField] private int maxMana = 90;
        [SerializeField] private int currentMana = 90;
        [SerializeField] private float regenPerSecond = 7f;
        [SerializeField] private float regenDelayAfterSpend = 1.4f;

        private float regenRemainder;
        private float regenPausedUntil;

        public int Current => currentMana;
        public int Max => maxMana;

        public void Configure(int max, float regen)
        {
            maxMana = Mathf.Max(1, max);
            currentMana = maxMana;
            regenPerSecond = Mathf.Max(0f, regen);
        }

        public bool Spend(int amount)
        {
            amount = Mathf.Max(0, amount);
            if (currentMana < amount)
            {
                return false;
            }

            currentMana -= amount;
            regenPausedUntil = Time.time + regenDelayAfterSpend;
            return true;
        }

        public void IncreaseMax(int amount)
        {
            amount = Mathf.Max(0, amount);
            maxMana += amount;
            currentMana = Mathf.Min(maxMana, currentMana + amount);
        }

        private void Update()
        {
            if (currentMana >= maxMana || regenPerSecond <= 0f || Time.time < regenPausedUntil)
            {
                return;
            }

            regenRemainder += regenPerSecond * Time.deltaTime;
            var wholeMana = Mathf.FloorToInt(regenRemainder);
            if (wholeMana <= 0)
            {
                return;
            }

            regenRemainder -= wholeMana;
            currentMana = Mathf.Min(maxMana, currentMana + wholeMana);
        }
    }
}
