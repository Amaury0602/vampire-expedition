using System;

namespace SaveSystem
{
    /// <summary>
    /// Base class for the plain serializable payload of a save. This is what gets written to
    /// disk — never the ScriptableObject itself, so authoring assets stay clean.
    /// </summary>
    [Serializable]
    public abstract class SaveData
    {
        public int Version;
    }
}
