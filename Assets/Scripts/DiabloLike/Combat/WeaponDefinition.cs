namespace DiabloLike.Combat
{
    public readonly struct WeaponDefinition
    {
        public readonly WeaponId Id;
        public readonly string Name;
        public readonly string Description;
        public readonly int Damage;
        public readonly float Cooldown;
        public readonly float Range;
        public readonly int ManaCost;

        public WeaponDefinition(WeaponId id, string name, string description, int damage, float cooldown, float range, int manaCost)
        {
            Id = id;
            Name = name;
            Description = description;
            Damage = damage;
            Cooldown = cooldown;
            Range = range;
            ManaCost = manaCost;
        }
    }
}
