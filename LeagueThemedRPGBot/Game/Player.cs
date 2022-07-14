using System.Text.Json.Serialization;

namespace LeagueThemedRPGBot.Game
{
    [JsonSerializable(typeof(Item))]
    [JsonSerializable(typeof(List<Item>))]
    [JsonSerializable(typeof(Skill))]
    [JsonSerializable(typeof(List<Skill>))]
    public class Player
    {
        // instanced data
        public int Level { get; set; } = 1;
        public int XP { get; set; } = 0;
        public int MaxHealth { get; set; } = 50;
        public int Health { get; set; } = 50;
        public int MaxMana { get; set; } = 10;
        public int Mana { get; set; } = 10;
        public int AttackDamage { get; set; } = 10;
        public int AbilityPower { get; set; } = 0;
        public int CritChance { get; set; } = 0;
        public int BonusCritDamage { get; set; } = 0;
        public int ArmorPenPercent { get; set; } = 0;
        public int ArmorPenFlat { get; set; } = 0;
        public int MagicPenPercent { get; set; } = 0;
        public int MagicPenFlat { get; set; } = 0;
        public int Omnivamp { get; set; } = 0;
        public int Armor { get; set; } = 5;
        public int MagicResist { get; set; } = 5;
        public int Gold { get; set; } = 0;
        public bool Busy { get; set; } = false;

        [JsonInclude]
        public List<Item> Inventory { get; set; } = new();

        [JsonInclude]
        public Item MainWeapon { get; set; }

        [JsonInclude]
        public Item OffhandWeapon { get; set; }

        [JsonInclude]
        public Item Armor1 { get; set; }

        [JsonInclude]
        public Item Armor2 { get; set; }

        [JsonInclude]
        public Item Boots { get; set; }

        [JsonInclude]
        public List<Skill> KnownSkills { get; set; } = new();

        [JsonInclude]
        public Skill Skill1 { get; set; }

        [JsonInclude]
        public Skill Skill2 { get; set; }

        [JsonInclude]
        public Skill Skill3 { get; set; }
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

        public int CalculateXPForNextLevel()
        {
            return 100 + (Level * (Level * 14));
        }
    }
}
