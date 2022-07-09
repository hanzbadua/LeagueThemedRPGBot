

namespace LeagueThemedRPGBot.Game
{
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemRarity Rarity { get; set; }
        public ItemType Type { get; set; }
        public int Value { get; set; }
        public ItemStats Stats { get; set; } = null;
    }
}
