using DiabloLike.Combat;
using UnityEngine;

namespace DiabloLike.UI
{
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new(0f, 2.25f, 0f);
        [SerializeField] private Vector2 size = new(58f, 7f);

        private Camera mainCamera;
        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void OnGUI()
        {
            if (mainCamera == null || health == null || health.IsDead)
            {
                return;
            }

            var screen = mainCamera.WorldToScreenPoint(transform.position + worldOffset);
            if (screen.z <= 0f)
            {
                return;
            }

            var health01 = Mathf.Clamp01((float)health.Current / health.Max);
            var rect = new Rect(screen.x - size.x * 0.5f, Screen.height - screen.y, size.x, size.y);
            DrawRect(new Rect(rect.x - 1f, rect.y - 1f, rect.width + 2f, rect.height + 2f), new Color(0.03f, 0.01f, 0.01f, 0.95f));
            DrawRect(rect, new Color(0.18f, 0.02f, 0.015f, 0.95f));
            DrawRect(new Rect(rect.x, rect.y, rect.width * health01, rect.height), new Color(0.85f, 0.03f, 0.02f, 0.95f));
        }

        private static void DrawRect(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}
