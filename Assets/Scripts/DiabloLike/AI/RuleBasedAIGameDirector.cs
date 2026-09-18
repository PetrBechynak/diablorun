using UnityEngine;

namespace DiabloLike.AI
{
    public sealed class RuleBasedAIGameDirector : IAIGameDirector
    {
        public AIResponse Complete(AIRequest request)
        {
            return request.Feature switch
            {
                AIFeature.Chat => Chat(request),
                AIFeature.Quest => Quest(request),
                AIFeature.NpcBehavior => NpcBehavior(request),
                AIFeature.Encounter => Encounter(request),
                _ => new AIResponse("...")
            };
        }

        private static AIResponse Chat(AIRequest request)
        {
            if (request.World.EnemiesAlive > 0)
            {
                return new AIResponse("Drz se u okraje svetla. Stiny se stahuji, kdyz zustanes stat.");
            }

            return new AIResponse("Na chvili je ticho. Posbirej esence a priprav se na dalsi vlnu.");
        }

        private static AIResponse Quest(AIRequest request)
        {
            if (request.World.EnemiesAlive <= 0)
            {
                return new AIResponse("Quest hotov: kruh je ocisteny. Dalsi rana do temnoty bude tvrdsi.", "complete_quest");
            }

            return new AIResponse($"Znic jeste {request.World.EnemiesAlive} posedlych a udrz runovy kruh.", "continue_quest");
        }

        private static AIResponse NpcBehavior(AIRequest request)
        {
            var offset = Random.insideUnitCircle.normalized * Random.Range(3f, 7f);
            var target = request.World.PlayerPosition + new Vector3(offset.x, 0f, offset.y);
            return new AIResponse("Lovec obchazi hrace a hleda uhel k utoku.", "flank_player", target);
        }

        private static AIResponse Encounter(AIRequest request)
        {
            var shouldSpawn = request.World.EnemiesAlive < 4 ? "spawn_wave" : "wait";
            return new AIResponse("Rezie encounteru drzi tlak bez zahlceni areny.", shouldSpawn);
        }
    }
}
