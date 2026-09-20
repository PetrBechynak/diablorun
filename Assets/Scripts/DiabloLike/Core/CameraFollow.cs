using UnityEngine;

namespace DiabloLike.Core
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 16.9f, -13f);
        [SerializeField] private float smoothTime = 0.12f;

        private Vector3 velocity;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime);
        }
    }
}
