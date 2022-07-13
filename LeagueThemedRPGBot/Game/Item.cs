using System.Text.Json.Serialization;

namespace LeagueThemedRPGBot.Game
{
    [JsonSerializable(typeof(ItemStats))]
    public class Item
    {
        public string Name { get; set; } = "Debug (if you see this something broke)";
        public string Description { get; set; } = "A Description";
        public ItemRarity Rarity { get; set; }
        public ItemType Type { get; set; }
        public int Value { get; set; }
        public ItemUseEffect UseEffect { get; set; }
        public ItemStats Stats { get; set; } = null;

        public override string ToString() => Name;
    }
}
