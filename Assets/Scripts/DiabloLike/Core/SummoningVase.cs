using DiabloLike.Combat;
using UnityEngine;

namespace DiabloLike.Core
{
    [RequireComponent(typeof(Health))]
    public sealed class SummoningVase : MonoBehaviour
    {
        private GameDirector director;
        private Health health;

        public bool IsAlive => health != null && !health.IsDead;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += OnDied;
        }

        public void Configure(GameDirector gameDirector)
        {
            director = gameDirector;
        }

        private void OnDied(Health deadHealth)
        {
            director?.OnVaseDestroyed(this);
        }
    }
}
