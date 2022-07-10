using System.Text.Json.Serialization;

namespace LeagueThemedRPGBot.Game
{
    [JsonSerializable(typeof(Item))]
    [JsonSerializable(typeof(List<Item>))]
    public class Player
    {
        // instanced data
        public int Level { get; set; } = 1;
        public int XP { get; set; } = 0;
        public int MaxHealth { get; set; } = 100;
        public int Health { get; set; } = 100;
        public int MaxMana { get; set; } = 50;
        public int Mana { get; set; } = 50;
        public int AttackDamage { get; set; } = 10;
        public int AbilityPower { get; set; } = 0;
        public int CritChance { get; set; } = 0;
        public int BonusCritDamage { get; set; } = 0;
        public int ArmorPenPercent { get; set; } = 0;
        public int ArmorPenFlat { get; set; } = 0;
        public int MagicPenPercent { get; set; } = 0;
        public int MagicPenFlat { get; set; } = 0;
        public int Omnivamp { get; set; } = 0;
        public int Armor { get; set; } = 0;
        public int MagicResist { get; set; } = 0;
        public int Gold /*MorbBucks*/ { get; set; } = 100;
        public bool Busy { get; set; } = false;

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

        public void AddStatsFromItem(Item i)
        {
            if (i.Stats is null) return;

            var s = i.Stats;

            MaxHealth += s.MaxHealth;
            MaxMana += s.MaxMana;
            AttackDamage += s.AttackDamage;
            AbilityPower += s.AbilityPower;
            CritChance += s.CritChance;
            BonusCritDamage += s.CritDamage;
            ArmorPenPercent += s.ArmorPenPercent;
            ArmorPenFlat += s.ArmorPenFlat;
            MagicPenPercent += s.MagicPenPercent;
            MagicPenFlat += s.MagicPenFlat;
            Omnivamp += s.Omnivamp;
            Armor += s.Armor;
            MagicResist += s.MagicResist;
        }

        public void RemoveStatsFromItem(Item i)
        {
            if (i.Stats is null) return;

            var s = i.Stats;

            MaxHealth -= s.MaxHealth;
            MaxMana -= s.MaxMana;
            AttackDamage -= s.AttackDamage;
            AbilityPower -= s.AbilityPower;
            CritChance -= s.CritChance;
            BonusCritDamage -= s.CritDamage;
            ArmorPenPercent -= s.ArmorPenPercent;
            ArmorPenFlat -= s.ArmorPenFlat;
            MagicPenPercent -= s.MagicPenPercent;
            MagicPenFlat -= s.MagicPenFlat;
            Omnivamp -= s.Omnivamp;
            Armor -= s.Armor;
            MagicResist -= s.MagicResist;
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
