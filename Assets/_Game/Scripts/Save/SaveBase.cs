using Sirenix.OdinInspector;
using System;
using System.IO;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Base class for save slots. The ScriptableObject only holds authoring data (default values,
    /// the save key); the actual runtime state lives in <see cref="Data"/>, a plain [Serializable]
    /// DTO that is what gets written to / read from disk. This keeps the .asset itself untouched
    /// by gameplay — playing in the editor never dirties the checked-in asset.
    /// </summary>
    public abstract class SaveBase<TData> : SaveBase where TData : SaveData, new()
    {
        [NonSerialized] public TData Data;

        public sealed override void Init()
        {
            Data = new TData { Version = CurrentVersion };
            OnInit(Data);
        }

        public sealed override void Save()
        {
            OnBeforeSave(Data);
            try
            {
                string json = JsonUtility.ToJson(Data, prettyPrint: true);
                File.WriteAllText(FilePath, json);
                Debug.Log($"[Save] '{SaveKey}' saved -> {FilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Failed to save '{SaveKey}': {e.Message}");
            }
        }

        public sealed override void Load()
        {
            if (!File.Exists(FilePath))
            {
                Debug.LogWarning($"[Save] No save file for '{SaveKey}', using defaults.");
                Init();
                OnAfterLoad(false);
                return;
            }

            try
            {
                TData loaded = JsonUtility.FromJson<TData>(File.ReadAllText(FilePath));
                if (loaded.Version != CurrentVersion)
                    Migrate(loaded, loaded.Version);

                Data = loaded;
                Debug.Log($"[Save] '{SaveKey}' loaded from {FilePath}");
                OnAfterLoad(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Failed to load '{SaveKey}': {e.Message}");
                Init();
                OnAfterLoad(false);
            }
        }

        public sealed override void Delete()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                Debug.Log($"[Save] File '{SaveKey}' deleted.");
            }
            Init();
        }

        public sealed override bool HasSaveFile() => File.Exists(FilePath);

        /// <summary>The current schema version. Bump this when TData's shape changes and add a case in Migrate.</summary>
        protected virtual int CurrentVersion => 0;

        /// <summary>Called on first launch or after a Delete. Set default values on the fresh data here.</summary>
        protected virtual void OnInit(TData data) { }

        /// <summary>Called just before serialization. Capture live runtime state into Data here if needed.</summary>
        protected virtual void OnBeforeSave(TData data) { }

        /// <summary>Called after deserialization (or after falling back to defaults). Apply Data to runtime here.</summary>
        /// <param name="success">False if no file existed or loading failed.</param>
        protected virtual void OnAfterLoad(bool success) { }

        /// <summary>Upgrade a loaded save from an older version in place. Override when CurrentVersion changes.</summary>
        protected virtual void Migrate(TData data, int fromVersion) => data.Version = CurrentVersion;
    }

    /// <summary>
    /// Non-generic base so <see cref="SaveMaster"/> can hold a single polymorphic array of save slots.
    /// </summary>
    public abstract class SaveBase : ScriptableObject
    {
        [Tooltip("Save file name (without extension). Leave empty to use the asset name.")]
        [SerializeField] private string _saveKey = "";

        public string SaveKey => string.IsNullOrEmpty(_saveKey) ? name : _saveKey;
        public string FilePath => Path.Combine(Application.persistentDataPath, SaveKey + ".json");

        public abstract void Init();
        public abstract void Save();
        public abstract void Load();
        public abstract void Delete();
        public abstract bool HasSaveFile();

        [Button("Save")] private void EditorSave() => Save();
        [Button("Load")] private void EditorLoad() => Load();
    }
}
