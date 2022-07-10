using System.Text.Json.Serialization;

namespace LeagueThemedRPGBot.Game
{
    [JsonSerializable(typeof(Item))]
    [JsonSerializable(typeof(List<Item>))]
    public class Player
    {
        // instanced data
        public int Level { get; set; } = 1;
        public float XP { get; set; } = 0;
        public int MaxHealth { get; set; } = 100;
        public int Health { get; set; } = 100;
        public int MaxMana { get; set; } = 50;
        public int Mana { get; set; } = 50;
        public int AttackDamage { get; set; } = 10;
        public int AbilityPower { get; set; } = 0;
        public int CritChance { get; set; } = 0;
        public int CritDamage { get; set; } = 175;
        public int ArmorPenPercent { get; set; } = 0;
        public int ArmorPenFlat { get; set; } = 0;
        public int MagicPenPercent { get; set; } = 0;
        public int MagicPenFlat { get; set; } = 0;
        public int Omnivamp { get; set; } = 0;
        public int Armor { get; set; } = 0;
        public int MagicResist { get; set; } = 0;
        public int Gold /*MorbBucks*/ { get; set; } = 100;

        [JsonInclude]
        public List<Item> Inventory { get; set; } = new();

        [JsonInclude]
        public Item MainWeapon { get; set; }

        [JsonInclude]
        public Item OffhandWeapon { get; set; }

        [JsonInclude]
        public Item ArmorOne { get; set; }

        [JsonInclude]
        public Item ArmorTwo { get; set; }

        [JsonInclude]
        public Item ArmorThree { get; set; }

        [JsonInclude]
        public Item Boots { get; set; }

        // statics
        public static Dictionary<ulong, Player> Data { get; set; }

        public static Player AddStatsFromItem(Player p, Item i)
        {
            var s = i.Stats;

            p.MaxHealth += s.MaxHealth;
            p.MaxMana += s.MaxMana;
            p.AttackDamage += s.AttackDamage;
            p.AbilityPower += s.AbilityPower;
            p.CritChance += s.CritChance;
            p.CritDamage += s.CritDamage;
            p.ArmorPenPercent += s.ArmorPenPercent;
            p.ArmorPenFlat += s.ArmorPenFlat;
            p.MagicPenPercent += s.MagicPenPercent;
            p.MagicPenFlat += s.MagicPenFlat;
            p.Omnivamp += s.Omnivamp;
            p.Armor += s.Armor;
            p.MagicResist += s.MagicResist;

            return p;
        }

        public static Player RemoveStatsFromItem(Player p, Item i)
        {
            var s = i.Stats;

            p.MaxHealth -= s.MaxHealth;
            p.MaxMana -= s.MaxMana;
            p.AttackDamage -= s.AttackDamage;
            p.AbilityPower -= s.AbilityPower;
            p.CritChance -= s.CritChance;
            p.CritDamage -= s.CritDamage;
            p.ArmorPenPercent -= s.ArmorPenPercent;
            p.ArmorPenFlat -= s.ArmorPenFlat;
            p.MagicPenPercent -= s.MagicPenPercent;
            p.MagicPenFlat -= s.MagicPenFlat;
            p.Omnivamp -= s.Omnivamp;
            p.Armor -= s.Armor;
            p.MagicResist -= s.MagicResist;

            return p;
        }

        public static float CalculateXPForNextLevel(int currentLevel)
        {
            return 100 + (currentLevel * (currentLevel * 12.5f));
        }

        public static bool PlayerIsAlreadyInitialized(ulong playerID)
        {
            return Data.ContainsKey(playerID);
        }
    }
}
