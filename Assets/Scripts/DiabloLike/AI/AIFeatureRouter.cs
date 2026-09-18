namespace DiabloLike.AI
{
    public sealed class AIFeatureRouter
    {
        private IAIGameDirector director = new RuleBasedAIGameDirector();

        public void UseDirector(IAIGameDirector nextDirector)
        {
            if (nextDirector != null)
            {
                director = nextDirector;
            }
        }

        public AIResponse Ask(AIRequest request)
        {
            return director.Complete(request);
        }
    }
}
