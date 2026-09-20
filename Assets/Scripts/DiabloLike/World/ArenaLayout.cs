using UnityEngine;

namespace DiabloLike.World
{
    /// <summary>Static scene/prefab data consumed by the runtime game director.</summary>
    public sealed class ArenaLayout : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform[] vaseSpawns = new Transform[0];
        [SerializeField] private Transform chestSpawn;
        [SerializeField] private Transform portalSpawn;

        public Transform PlayerSpawn => playerSpawn != null ? playerSpawn : transform;
        public Transform[] VaseSpawns => vaseSpawns ?? new Transform[0];
        public Transform ChestSpawn => chestSpawn != null ? chestSpawn : transform;
        public Transform PortalSpawn => portalSpawn != null ? portalSpawn : transform;

        public void Configure(Transform player, Transform[] vases, Transform chest, Transform portal)
        {
            playerSpawn = player;
            vaseSpawns = vases ?? new Transform[0];
            chestSpawn = chest;
            portalSpawn = portal;
        }
    }
}
