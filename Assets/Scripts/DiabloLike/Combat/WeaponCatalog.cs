namespace DiabloLike.Combat
{
    public static class WeaponCatalog
    {
        public static readonly WeaponDefinition Sword = new(
            WeaponId.Sword,
            "Iron Sword",
            "Close arc, fast melee.",
            35,
            0.35f,
            1.1f,
            0);

        public static readonly WeaponDefinition MagicWand = new(
            WeaponId.MagicWand,
            "Apprentice Wand",
            "Short ranged spell bolt.",
            52,
            0.65f,
            3.4f,
            8);

        public static readonly WeaponDefinition EmberWand = new(
            WeaponId.EmberWand,
            "Ember Wand",
            "Upgraded spell reach.",
            78,
            0.52f,
            4.6f,
            6);

        public static WeaponDefinition Get(WeaponId id)
        {
            return id switch
            {
                WeaponId.MagicWand => MagicWand,
                WeaponId.EmberWand => EmberWand,
                _ => Sword
            };
        }
    }
}
