namespace LeagueThemedRPGBot.Game
{
    public class ItemData
    {
        private const string WeaponDataDirectory = "weapons";

        public Dictionary<string, Item> Weapons { get; }

        public ItemData()
        {
            Weapons = DataFunctions.LoadGameDataFromDirectory<Item>(WeaponDataDirectory);
        }

        public Item GetWeaponByName(string name)
        {
            name = name.RemoveWhitespace().ToLowerInvariant();
            if (Weapons.ContainsKey(name))
                return Weapons[name];

            return FailsafeLongSword;
        }

        public static Item FailsafeLongSword { get; } = new()
        {
            Name = "Long Sword",
            Description = "It hurts.",
            Rarity = ItemRarity.Basic,
            Type = ItemType.Weapon,
            Stats = new()
            {
                AttackDamage = 10
            }
        };
    }
}
