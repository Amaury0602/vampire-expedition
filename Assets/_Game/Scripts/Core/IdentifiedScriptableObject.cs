using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Base for ScriptableObjects that need a stable identity in save data. JsonUtility cannot
    /// round-trip a direct UnityEngine.Object reference outside of a scene/prefab context, so
    /// anything referenced from a save file (Currency, PerkDefinition, ...) is looked up by this
    /// Id string instead of by object reference. The Id is a GUID generated once on creation and
    /// never touched afterward — renaming or moving the asset does not change it.
    /// </summary>
    public abstract class IdentifiedScriptableObject : ScriptableObject
    {
        [ReadOnly, SerializeField] private string _id;

        public string Id => _id;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = System.Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
