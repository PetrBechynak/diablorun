using UnityEngine;

namespace DiabloLike.Tests
{
    public sealed class SlashCombatTestDriver : MonoBehaviour
    {
        [SerializeField] private float attackInterval = 0.85f;
        private float nextAttack;

        private void Update()
        {
            if (Time.time < nextAttack)
            {
                return;
            }

            var player = GameObject.Find("Player - Rune Hunter");
            if (player == null)
            {
                return;
            }

            nextAttack = Time.time + attackInterval;
            var controller = player.GetComponent<DiabloLike.Core.PlayerController>();
            if (controller != null)
            {
                controller.TriggerAttackForTest();
            }
        }
    }
}
