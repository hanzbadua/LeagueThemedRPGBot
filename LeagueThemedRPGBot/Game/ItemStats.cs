using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueThemedRPGBot.Game
{
    public class ItemStats
    {
        public int MaxHealth { get; set; } = 0;
        public int MaxMana { get; set; } = 0;
        public int AttackDamage { get; set; } = 0;
        public int AbilityPower { get; set; } = 0;
        public int CritChance { get; set; } = 0;
        public int CritDamage { get; set; } = 0;
        public int ArmorPenPercent { get; set; } = 0;
        public int ArmorPenFlat { get; set; } = 0;
        public int MagicPenPercent { get; set; } = 0;
        public int MagicPenFlat { get; set; } = 0;
        public int Omnivamp { get; set; } = 0;
        public int Armor { get; set; } = 0;
        public int MagicResist { get; set; } = 0;
        public ItemEffect Effect { get; set; } = null;
    }
}
