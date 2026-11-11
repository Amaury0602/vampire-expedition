using Characters;

namespace Skills
{
    public class SkillSlot
    {
        public Hero Owner { get; private set; }
        public SkillSettings Settings { get; private set; }

        public SkillSlot(Hero owner, SkillSettings settings)
        {
            Owner = owner;
            Settings = settings;
        }
    }
}
