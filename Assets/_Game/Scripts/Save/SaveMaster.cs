using Sirenix.OdinInspector;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Central registry of every save slot in the game. Create one via
    /// Assets -> Create -> Save System -> Save Master and reference it from GameConfig.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveMaster", menuName = "SO/Save/SaveMaster")]
    public class SaveMaster : ScriptableObject
    {
        [SerializeField] private SaveBase[] _saves = System.Array.Empty<SaveBase>();

        public void SaveAll()
        {
            for (int i = 0; i < _saves.Length; i++)
                _saves[i]?.Save();
        }

        public void LoadAll()
        {
            for (int i = 0; i < _saves.Length; i++)
                _saves[i]?.Load();
        }

        public void DeleteAll()
        {
            for (int i = 0; i < _saves.Length; i++)
                _saves[i]?.Delete();
        }

        public T Get<T>() where T : SaveBase
        {
            for (int i = 0; i < _saves.Length; i++)
                if (_saves[i] is T match)
                    return match;

            Debug.LogError($"[SaveMaster] No save of type {typeof(T).Name} registered.");
            return null;
        }

        public SaveBase[] GetAllSaves() => _saves;

#if UNITY_EDITOR
        [Button("Find all saves")]
        private void FindAllSaves()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SaveBase");
            var found = new System.Collections.Generic.List<SaveBase>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                var save = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveBase>(path);
                if (save != null)
                    found.Add(save);
            }
            _saves = found.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[SaveMaster] Found {_saves.Length} save asset(s).");
        }

        [Button("Save All")] private void EditorSaveAll() => SaveAll();
        [Button("Load All")] private void EditorLoadAll() => LoadAll();
        [Button("Delete All")] private void EditorDeleteAll() => DeleteAll();
#endif
    }
}
