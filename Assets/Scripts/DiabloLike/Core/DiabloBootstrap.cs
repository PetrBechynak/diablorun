using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiabloLike.Core
{
    public static class DiabloBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            // The isolated VFX test scene must not start the gameplay bootstrap.
            if (SceneManager.GetActiveScene().name == "SlashVfxTest")
            {
                return;
            }

            if (Object.FindFirstObjectByType<GameDirector>() != null)
            {
                return;
            }

            var director = new GameObject("DiabloLike Game Director");
            Object.DontDestroyOnLoad(director);
            director.AddComponent<GameDirector>();
        }
    }
}
