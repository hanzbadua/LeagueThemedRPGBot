namespace LeagueThemedRPGBot.Game
{
    public class Player
    {
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
        public List<Item> Inventory { get; set; } = new();
        public Item Weapon1 { get; set; }
        public Item Weapon2 { get; set; }

    }
}
