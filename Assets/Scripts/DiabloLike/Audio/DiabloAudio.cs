using System.Collections.Generic;
using UnityEngine;

namespace DiabloLike.Audio
{
    public enum GameSfx
    {
        LevelStart,
        SwordHit,
        EnemyHurt,
        EnemyDeath,
        EnemySpawn,
        ChestOpen,
        ChestLoot,
        PortalOpen,
        Teleport,
        WandCast,
        WandHit,
        UiClick
    }

    public sealed class DiabloAudio : MonoBehaviour
    {
        private static readonly Dictionary<GameSfx, string[]> ClipPaths = new()
        {
            [GameSfx.LevelStart] = new[] { "Audio/SFX/level_start_01" },
            [GameSfx.SwordHit] = new[] { "Audio/SFX/sword_hit_01", "Audio/SFX/sword_hit_02", "Audio/SFX/sword_hit_03" },
            [GameSfx.EnemyHurt] = new[] { "Audio/SFX/enemy_hurt_01", "Audio/SFX/enemy_hurt_02" },
            [GameSfx.EnemyDeath] = new[] { "Audio/SFX/enemy_death_01" },
            [GameSfx.EnemySpawn] = new[] { "Audio/SFX/enemy_spawn_01", "Audio/SFX/enemy_spawn_02" },
            [GameSfx.ChestOpen] = new[] { "Audio/SFX/chest_open_01" },
            [GameSfx.ChestLoot] = new[] { "Audio/SFX/chest_loot_01" },
            [GameSfx.PortalOpen] = new[] { "Audio/SFX/portal_open_01" },
            [GameSfx.Teleport] = new[] { "Audio/SFX/teleport_01" },
            [GameSfx.WandCast] = new[] { "Audio/SFX/wand_cast_01" },
            [GameSfx.WandHit] = new[] { "Audio/SFX/wand_hit_01" },
            [GameSfx.UiClick] = new[] { "Audio/SFX/ui_click_01" }
        };

        private static readonly Dictionary<GameSfx, float> Volumes = new()
        {
            [GameSfx.LevelStart] = 0.55f,
            [GameSfx.SwordHit] = 0.78f,
            [GameSfx.EnemyHurt] = 0.68f,
            [GameSfx.EnemyDeath] = 0.8f,
            [GameSfx.EnemySpawn] = 0.62f,
            [GameSfx.ChestOpen] = 0.78f,
            [GameSfx.ChestLoot] = 0.72f,
            [GameSfx.PortalOpen] = 0.68f,
            [GameSfx.Teleport] = 0.82f,
            [GameSfx.WandCast] = 0.5f,
            [GameSfx.WandHit] = 0.62f,
            [GameSfx.UiClick] = 0.52f
        };

        private static DiabloAudio instance;
        private readonly Dictionary<GameSfx, AudioClip[]> loadedClips = new();
        private readonly Dictionary<GameSfx, int> nextClipIndices = new();
        private AudioSource source;
        private float nextCrowdSoundTime;

        public static void Ensure()
        {
            if (instance != null)
            {
                EnsureListener();
                return;
            }

            var audioObject = new GameObject("Diablo Audio Bus");
            DontDestroyOnLoad(audioObject);
            instance = audioObject.AddComponent<DiabloAudio>();
            EnsureListener();
            AudioListener.pause = false;
            AudioListener.volume = 1f;
        }

        public static void Play(GameSfx sfx, float pitchJitter = 0.05f)
        {
            Ensure();
            if (IsCrowdLimited(sfx) && Time.time < instance.nextCrowdSoundTime)
            {
                return;
            }

            if (IsCrowdLimited(sfx))
            {
                instance.nextCrowdSoundTime = Time.time + 0.08f;
            }

            var clip = instance.NextClip(sfx);
            if (clip == null)
            {
                return;
            }

            instance.source.pitch = Random.Range(1f - pitchJitter, 1f + pitchJitter);
            instance.source.PlayOneShot(clip, Volumes.TryGetValue(sfx, out var volume) ? volume : 0.7f);
        }

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            source.priority = 48;
            source.ignoreListenerPause = true;
            AudioListener.volume = 1f;
            AudioListener.pause = false;
            PreloadClips();
        }

        private static bool IsCrowdLimited(GameSfx sfx)
        {
            return sfx == GameSfx.EnemyHurt || sfx == GameSfx.EnemySpawn || sfx == GameSfx.SwordHit || sfx == GameSfx.WandHit;
        }

        private void PreloadClips()
        {
            foreach (var pair in ClipPaths)
            {
                var clips = new List<AudioClip>();
                foreach (var path in pair.Value)
                {
                    var clip = Resources.Load<AudioClip>(path);
                    if (clip != null)
                    {
                        clips.Add(clip);
                    }
                    else
                    {
                        Debug.LogWarning($"Missing SFX clip at Resources/{path}");
                    }
                }

                loadedClips[pair.Key] = clips.ToArray();
                nextClipIndices[pair.Key] = Random.Range(0, Mathf.Max(1, clips.Count));
            }
        }

        private AudioClip NextClip(GameSfx sfx)
        {
            if (!loadedClips.TryGetValue(sfx, out var clips) || clips.Length == 0)
            {
                return null;
            }

            var index = nextClipIndices[sfx] % clips.Length;
            nextClipIndices[sfx] = index + 1;
            return clips[index];
        }

        private static void EnsureListener()
        {
            AudioListener.volume = 1f;
            AudioListener.pause = false;
            if (UnityEngine.Object.FindAnyObjectByType<AudioListener>() != null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera != null)
            {
                camera.gameObject.AddComponent<AudioListener>();
                return;
            }

            var listenerObject = new GameObject("Runtime Audio Listener");
            listenerObject.AddComponent<AudioListener>();
        }
    }
}
