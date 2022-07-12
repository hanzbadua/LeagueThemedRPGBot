namespace LeagueThemedRPGBot.Game
{
    public class Enemy
    {
        public string Name { get; set; } = "Debug (if you see this something broke)";
        public int MaxHealth { get; set; } = 5;
        public int Health { get; set; } = 5;
        public int AttackDamage { get; set; } = 5;
        public int AbilityPower { get; set; } = 5;
        public int Armor { get; set; } = 5;
        public int MagicResist { get; set; } = 5;

        public override string ToString() => Name;

        // Statics
        private static readonly List<(string, EncounterDamageType)> CommonEncounters = new()
        {
            ("Caster Minion", EncounterDamageType.Physical), ("Melee Minion", EncounterDamageType.Physical), ("Raptor", EncounterDamageType.Physical)
        };

        private static readonly List<(string, EncounterDamageType)> CommonBigEncounters = new()
        {
            ("Crimson Raptor", EncounterDamageType.Physical), ("Gromp", EncounterDamageType.Physical)
        };

        public static Enemy GetScalingEnemy(int playerLevel, EncounterTypes type, Random rng)
        {
            float typeMult = 0.8f;
            float levelMult = playerLevel / 1.2f;

            (string, EncounterDamageType) enc = ("Caster Minion", EncounterDamageType.Physical); // fallback assignment
            var dtype = enc.Item2;

            if (type == EncounterTypes.Common)
            {
                typeMult = 1.0f;
                enc = CommonEncounters[rng.Next(CommonEncounters.Count)];
            }
            else if (type == EncounterTypes.Uncommon)
            {
                typeMult = 1.22f;
                enc = CommonBigEncounters[rng.Next(CommonBigEncounters.Count)];
            }

            float overallMult = levelMult * typeMult;

            int hp = rng.Next((int)(100 * overallMult / 2), (int)(100 * overallMult * 2));

            var retval = new Enemy
            {
                Name = enc.Item1,
                MaxHealth = hp,
                Health = hp,
            };

            if (dtype == EncounterDamageType.Physical || dtype == EncounterDamageType.Mixed)
                retval.AttackDamage = rng.Next((int)(10 + (20 * overallMult)), (int)(10 + (20 * overallMult * 2)));

            if (dtype == EncounterDamageType.Mixed)
                retval.AbilityPower = rng.Next((int)(5 + (10 * overallMult)), (int)(10 + (20 * overallMult)));

            if (dtype == EncounterDamageType.Magic)
                retval.AbilityPower = rng.Next((int)(10 + (15 * overallMult)), (int)(15 + (30 * overallMult)));

            retval.Armor = rng.Next(3 + (int)(10 * overallMult));
            retval.MagicResist = rng.Next(3 + (int)(10 * overallMult));

            return retval;
        }
    }
}
