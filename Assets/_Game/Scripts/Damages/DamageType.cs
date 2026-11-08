using System;

namespace Damages
{
    [Flags]
    public enum DamageType
    {
        None = 0,
        Slash = 1 << 0,
        Pierce = 1 << 1,
        Blunt = 1 << 2,
        Fire = 1 << 3,
        Poison = 1 << 4
    }
}
