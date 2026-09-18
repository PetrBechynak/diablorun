using UnityEngine;

namespace DiabloLike.World
{
    public sealed class FloatingLoot : MonoBehaviour
    {
        private Vector3 basePosition;
        private float phase;

        private void Start()
        {
            basePosition = transform.position;
            phase = Random.Range(0f, 10f);
            Destroy(gameObject, 12f);
        }

        private void Update()
        {
            transform.position = basePosition + Vector3.up * Mathf.Sin(Time.time * 3f + phase) * 0.12f;
            transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        }
    }
}
