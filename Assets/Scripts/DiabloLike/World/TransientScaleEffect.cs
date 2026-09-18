using UnityEngine;

namespace DiabloLike.World
{
    public sealed class TransientScaleEffect : MonoBehaviour
    {
        private float duration = 0.15f;
        private float age;
        private Vector3 startScale = Vector3.one;
        private Vector3 endScale = Vector3.one;

        public void Configure(float lifeTime, Vector3 fromScale, Vector3 toScale)
        {
            duration = Mathf.Max(0.01f, lifeTime);
            startScale = fromScale;
            endScale = toScale;
        }

        private void Update()
        {
            age += Time.deltaTime;
            var t = Mathf.Clamp01(age / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            if (age >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
