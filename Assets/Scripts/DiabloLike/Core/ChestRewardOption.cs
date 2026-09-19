namespace DiabloLike.Core
{
    using UnityEngine;

    public readonly struct ChestRewardOption
    {
        public readonly ChestRewardId Id;
        public readonly string Title;
        public readonly string Body;
        public readonly bool IsRare;
        public readonly bool IsSuperRare;
        public readonly Color TitleColor;

        public ChestRewardOption(ChestRewardId id, string title, string body, bool isRare = false)
        {
            Id = id;
            Title = title;
            Body = body;
            IsRare = isRare;
            IsSuperRare = id == ChestRewardId.SuperRareHardenedBullet;
            TitleColor = IsSuperRare ? new Color(1f, 0.78f, 0.08f) : isRare ? new Color(0.95f, 0.2f, 1f) : Color.white;
        }
    }
}
