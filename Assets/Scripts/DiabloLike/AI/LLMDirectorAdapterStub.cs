namespace DiabloLike.AI
{
    public sealed class LLMDirectorAdapterStub : IAIGameDirector
    {
        private readonly IAIGameDirector fallback = new RuleBasedAIGameDirector();

        public AIResponse Complete(AIRequest request)
        {
            // Future hook: serialize AIRequest, call a model/backend, validate AIResponse, then return it.
            return fallback.Complete(request);
        }
    }
}
