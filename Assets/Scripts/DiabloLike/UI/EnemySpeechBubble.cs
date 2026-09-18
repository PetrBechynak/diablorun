using UnityEngine;

namespace DiabloLike.UI
{
    public sealed class EnemySpeechBubble : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new(0f, 2.65f, 0f);
        [SerializeField] private float visibleSeconds = 2.35f;

        private Camera mainCamera;
        private string text;
        private float hideAt;

        public void Say(string nextText)
        {
            if (string.IsNullOrWhiteSpace(nextText))
            {
                return;
            }

            text = nextText;
            hideAt = Time.time + visibleSeconds;
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
            if (mainCamera == null || string.IsNullOrEmpty(text) || Time.time >= hideAt)
            {
                return;
            }

            var screen = mainCamera.WorldToScreenPoint(transform.position + worldOffset);
            if (screen.z <= 0f)
            {
                return;
            }

            var width = Mathf.Clamp(text.Length * 7.5f + 24f, 120f, 260f);
            var height = 42f;
            var rect = new Rect(screen.x - width * 0.5f, Screen.height - screen.y - height, width, height);
            DrawRect(rect, new Color(0.02f, 0.015f, 0.012f, 0.9f));
            DrawRect(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f), new Color(0.12f, 0.08f, 0.055f, 0.95f));

            var oldColor = GUI.color;
            GUI.color = new Color(1f, 0.92f, 0.82f, 1f);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, rect.height - 12f), text);
            GUI.color = oldColor;
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
