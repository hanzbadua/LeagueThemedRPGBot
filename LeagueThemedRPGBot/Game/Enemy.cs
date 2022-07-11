namespace LeagueThemedRPGBot.Game
{
    public class Enemy
    {
        public string Name { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public int AttackDamage { get; set; }
        public int AbilityPower { get; set; }
        public int Armor { get; set; }
        public int MagicResist { get; set; }

        // Statics
        private static readonly List<(string, EncounterDamageType)> CommonEncounters = new()
        {
            ("Caster Minion", EncounterDamageType.Physical), ("Melee Minion", EncounterDamageType.Physical), ("Raptor", EncounterDamageType.Physical)
        };

        private static readonly List<(string, EncounterDamageType)> CommonBigEncounters = new()
        {
            ("Crimson Raptor", EncounterDamageType.Physical), ("Gromp", EncounterDamageType.Physical)
        };

        public static Enemy GetScalingEnemy(int playerLevel, EncounterTypes type)
        {
            float typeMult = 0.8f;
            float levelMult = playerLevel / 1.2f;

            (string, EncounterDamageType) enc = ("Caster Minion", EncounterDamageType.Physical); // fallback assignment
            var dtype = enc.Item2;

            if (type == EncounterTypes.Common)
            {
                typeMult = 1.0f;
                enc = CommonEncounters[DataFunctions.Rng.Next(CommonEncounters.Count)];
            }
            else if (type == EncounterTypes.Uncommon)
            {
                typeMult = 1.22f;
                enc = CommonBigEncounters[DataFunctions.Rng.Next(CommonBigEncounters.Count)];
            }

            float overallMult = levelMult * typeMult;

            int hp = DataFunctions.Rng.Next((int)(100 * overallMult / 2), (int)(100 * overallMult * 2));

            var retval = new Enemy
            {
                Name = enc.Item1,
                MaxHealth = hp,
                Health = hp,
            };

            if (dtype == EncounterDamageType.Physical || dtype == EncounterDamageType.Mixed)
                retval.AttackDamage = DataFunctions.Rng.Next((int)(10 + (20 * overallMult)), (int)(10 + (20 * overallMult * 2)));

            if (dtype == EncounterDamageType.Mixed)
                retval.AbilityPower = DataFunctions.Rng.Next((int)(5 + (10 * overallMult)), (int)(10 + (20 * overallMult)));

            if (dtype == EncounterDamageType.Magic)
                retval.AbilityPower = DataFunctions.Rng.Next((int)(10 + (15 * overallMult)), (int)(15 + (30 * overallMult)));

            retval.Armor = DataFunctions.Rng.Next(3 + (int)(10 * overallMult));
            retval.MagicResist = DataFunctions.Rng.Next(3 + (int)(10 * overallMult));

            return retval;
        }
    }
}
