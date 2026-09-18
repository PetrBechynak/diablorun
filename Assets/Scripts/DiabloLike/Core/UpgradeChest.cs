using DiabloLike.Audio;
using DiabloLike.World;
using UnityEngine;

namespace DiabloLike.Core
{
    public sealed class UpgradeChest : MonoBehaviour
    {
        private GameDirector director;
        private bool opened;

        public void Configure(GameDirector gameDirector)
        {
            director = gameDirector;
        }

        private void OnMouseDown()
        {
            Open();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                Open();
            }
        }

        private void Open()
        {
            if (opened)
            {
                return;
            }

            opened = true;
            DiabloAudio.Play(GameSfx.ChestOpen, 0.05f);
            EffectFactory.SpawnHit(transform.position + Vector3.up * 0.6f);
            director?.OnUpgradeChestOpened(this);
            Destroy(gameObject);
        }
    }
}
