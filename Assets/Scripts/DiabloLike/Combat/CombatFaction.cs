using UnityEngine;

namespace DiabloLike.Combat
{
    public enum Faction
    {
        Player,
        Enemy,
        Summoner
    }

    public sealed class CombatFaction : MonoBehaviour
    {
        [SerializeField] private Faction faction;

        public Faction Faction => faction;

        public void Configure(Faction nextFaction)
        {
            faction = nextFaction;
        }
    }
}
