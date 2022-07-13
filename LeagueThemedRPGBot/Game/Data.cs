namespace LeagueThemedRPGBot.Game
{
    public class Data
    {
        private const string WeaponDataDirectory = "weapons";
        private const string ArmorDataDirectory = "armor";
        private const string BootsDataDirectory = "boots";
        private const string EnemiesDataDirectory = "enemies";
        private const string SkillsDataDirectory = "skills";

        public Dictionary<string, Item> Weapons { get; }
        public Dictionary<string, Item> Armors { get; }
        public Dictionary<string, Item> Boots { get; }
        public Dictionary<string, Enemy> Enemies { get; }
        public Dictionary<string, Skill> Skills { get; }

        public Data()
        {
            Weapons = DataGlobals.LoadGameDataFromDirectory<Item>(WeaponDataDirectory);
            Armors = DataGlobals.LoadGameDataFromDirectory<Item>(ArmorDataDirectory);
            Boots = DataGlobals.LoadGameDataFromDirectory<Item>(BootsDataDirectory);
            Enemies = DataGlobals.LoadGameDataFromDirectory<Enemy>(EnemiesDataDirectory);
            Skills = DataGlobals.LoadGameDataFromDirectory<Skill>(SkillsDataDirectory);
        }

        public Item GetWeaponByName(string name)
        {
            name = name.RemoveWhitespace().ToLowerInvariant();
            if (Weapons.ContainsKey(name))
                return Weapons[name];

            return new Item();
        }
        public Item GetArmorByName(string name)
        {
            name = name.RemoveWhitespace().ToLowerInvariant();
            if (Armors.ContainsKey(name))
                return Armors[name];

            return new Item();
        }
        public Item GetBootsByName(string name)
        {
            name = name.RemoveWhitespace().ToLowerInvariant();
            if (Boots.ContainsKey(name))
                return Boots[name];

            return new Item();
        }

        public Enemy GetEnemyByName(string name)
        {
            name = name.RemoveWhitespace().ToLowerInvariant();
            if (Enemies.ContainsKey(name))
                return Enemies[name];

            return new Enemy();
        }

        public Skill GetSkillByName(string name)
        {
            name = name.RemoveWhitespace().ToLowerInvariant();
            if (Skills.ContainsKey(name))
                return Skills[name];

            return new Skill();
        }
    }
}
