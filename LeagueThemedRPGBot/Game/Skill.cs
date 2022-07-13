namespace LeagueThemedRPGBot.Game
{
    public class Skill
    {
        public string Name { get; set; } = "Debug (if you see this something broke)";
        public string Description { get; set; } = "A Description";
        public int ManaCost { get; set; } = 5;
        public int Cooldown { get; set; } = 0;
        public SkillCastRestriction CastRestriction { get; set; } = SkillCastRestriction.Normal;
        public SkillEffect Effect { get; set; } = SkillEffect.EmpoweredAttack;
        public override string ToString() => Name;
    }
}
