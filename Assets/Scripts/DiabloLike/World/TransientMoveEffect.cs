using UnityEngine;

namespace DiabloLike.World
{
    public sealed class TransientMoveEffect : MonoBehaviour
    {
        private float duration = 0.25f;
        private float age;
        private Vector3 start;
        private Vector3 end;

        public void Configure(float lifeTime, Vector3 startPosition, Vector3 endPosition)
        {
            duration = Mathf.Max(0.01f, lifeTime);
            start = startPosition;
            end = endPosition;
        }

        private void Update()
        {
            age += Time.deltaTime;
            var t = Mathf.Clamp01(age / duration);
            transform.position = Vector3.Lerp(start, end, 1f - Mathf.Pow(1f - t, 2f));
            transform.localScale *= 1f - Time.deltaTime * 2.8f;

            if (age >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
