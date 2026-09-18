using UnityEngine;

namespace DiabloLike.Core
{
    public static class DiabloBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<GameDirector>() != null)
            {
                return;
            }

            var director = new GameObject("DiabloLike Game Director");
            director.AddComponent<GameDirector>();
        }
    }
}
