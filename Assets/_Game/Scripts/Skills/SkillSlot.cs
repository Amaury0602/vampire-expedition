using System;
using Characters;

namespace Skills
{
    public class SkillSlot
    {
        public Hero Owner { get; private set; }
        public SkillSettings Settings { get; private set; }
        public int Level { get; private set; }

        public SkillLevel Data
        {
            get { return Settings.GetLevel(Level); }
        }

        public event Action<SkillSlot> LevelChanged;

        public SkillSlot(Hero owner, SkillSettings settings, int level = 1)
        {
            Owner = owner;
            Settings = settings;
            Level = settings.ClampLevel(level);
        }

        public void SetLevel(int level)
        {
            int clamped = Settings.ClampLevel(level);
            if (clamped == Level)
                return;

            Level = clamped;
            LevelChanged?.Invoke(this);
        }
    }
}
