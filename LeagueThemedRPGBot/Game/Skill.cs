namespace LeagueThemedRPGBot.Game
{
    public class Skill
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManaCost { get; set; } = 5;
        public int Cooldown { get; set; } = 0;
        public bool AppliesOnHits { get; set; } = false;
        public SkillCastRestriction CastRestriction { get; set; } = SkillCastRestriction.Normal;

        public SkillAction Action1 { get; set; } = SkillAction.None;
        public int Scaling1 { get; set; }
        public SkillScalingType ScalingType1 { get; set; } = SkillScalingType.None;

        public SkillAction Action2 { get; set; } = SkillAction.None;
        public int Scaling2 { get; set; }
        public SkillScalingType ScalingType2 { get; set; } = SkillScalingType.None;
    }
}
