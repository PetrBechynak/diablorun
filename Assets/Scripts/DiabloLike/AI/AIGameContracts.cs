using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiabloLike.AI
{
    public enum AIFeature
    {
        Chat,
        Quest,
        NpcBehavior,
        Encounter
    }

    [Serializable]
    public sealed class WorldSnapshot
    {
        public string PlayerName;
        public int PlayerLevel;
        public int PlayerHealth;
        public int PlayerMaxHealth;
        public int EnemiesAlive;
        public string CurrentQuestId;
        public Vector3 PlayerPosition;
        public readonly List<string> RecentEvents = new();
    }

    public readonly struct AIRequest
    {
        public readonly AIFeature Feature;
        public readonly string ActorId;
        public readonly string PlayerText;
        public readonly WorldSnapshot World;

        public AIRequest(AIFeature feature, string actorId, string playerText, WorldSnapshot world)
        {
            Feature = feature;
            ActorId = actorId;
            PlayerText = playerText;
            World = world;
        }
    }

    public readonly struct AIResponse
    {
        public readonly string Text;
        public readonly string Intent;
        public readonly Vector3 TargetPosition;

        public AIResponse(string text, string intent = "", Vector3 targetPosition = default)
        {
            Text = text;
            Intent = intent;
            TargetPosition = targetPosition;
        }
    }

    public interface IAIGameDirector
    {
        AIResponse Complete(AIRequest request);
    }
}
