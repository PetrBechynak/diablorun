using UnityEngine;

namespace DiabloLike.Core
{
    public sealed class LevelPortal : MonoBehaviour
    {
        private GameDirector director;

        public void Configure(GameDirector gameDirector)
        {
            director = gameDirector;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                director?.EnterNextLevel();
            }
        }
    }
}
