namespace DiabloLike.Core
{
    using UnityEngine;

    public readonly struct ChestRewardOption
    {
        public readonly ChestRewardId Id;
        public readonly string Title;
        public readonly string Body;
        public readonly bool IsRare;
        public readonly Color TitleColor;

        public ChestRewardOption(ChestRewardId id, string title, string body, bool isRare = false)
        {
            Id = id;
            Title = title;
            Body = body;
            IsRare = isRare;
            TitleColor = isRare ? new Color(0.95f, 0.2f, 1f) : Color.white;
        }
    }
}
