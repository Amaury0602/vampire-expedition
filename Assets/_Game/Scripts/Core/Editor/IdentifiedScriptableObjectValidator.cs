using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core
{
    public static class IdentifiedScriptableObjectValidator
    {
        [MenuItem("Tools/Validation/Check Duplicate IDs")]
        private static void CheckDuplicateIds()
        {
            Dictionary<string, List<string>> byId = CollectIdsByAssetPath();
            int duplicateCount = LogDuplicates(byId);

            if (duplicateCount == 0)
                Debug.Log($"No duplicate Ids found across {CountAssets(byId)} IdentifiedScriptableObject assets.");
        }

        [MenuItem("Tools/Validation/Fix Duplicate IDs (keep first, regenerate rest)")]
        private static void FixDuplicateIds()
        {
            Dictionary<string, List<string>> byId = CollectIdsByAssetPath();
            int fixedCount = 0;

            foreach (KeyValuePair<string, List<string>> pair in byId)
            {
                if (pair.Value.Count <= 1)
                    continue;

                for (int i = 1; i < pair.Value.Count; i++)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<IdentifiedScriptableObject>(pair.Value[i]);
                    var serialized = new SerializedObject(asset);
                    serialized.FindProperty("_id").stringValue = System.Guid.NewGuid().ToString("N");
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(asset);
                    Debug.Log($"Regenerated Id for {pair.Value[i]}", asset);
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
                AssetDatabase.SaveAssets();

            Debug.Log($"Fixed {fixedCount} duplicate IDs.");
        }

        private static Dictionary<string, List<string>> CollectIdsByAssetPath()
        {
            string[] guids = AssetDatabase.FindAssets("t:IdentifiedScriptableObject");
            var byId = new Dictionary<string, List<string>>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<IdentifiedScriptableObject>(path);
                if (asset == null)
                    continue;

                if (string.IsNullOrEmpty(asset.Id))
                {
                    Debug.LogWarning($"Empty Id: {path}", asset);
                    continue;
                }

                if (!byId.TryGetValue(asset.Id, out List<string> paths))
                {
                    paths = new List<string>();
                    byId[asset.Id] = paths;
                }
                paths.Add(path);
            }

            return byId;
        }

        private static int LogDuplicates(Dictionary<string, List<string>> byId)
        {
            int duplicateCount = 0;

            foreach (KeyValuePair<string, List<string>> pair in byId)
            {
                if (pair.Value.Count <= 1)
                    continue;

                duplicateCount++;
                Debug.LogError($"Duplicate Id '{pair.Key}' shared by {pair.Value.Count} assets:\n{string.Join("\n", pair.Value)}");
            }

            return duplicateCount;
        }

        private static int CountAssets(Dictionary<string, List<string>> byId)
        {
            int count = 0;
            foreach (KeyValuePair<string, List<string>> pair in byId)
                count += pair.Value.Count;
            return count;
        }
    }
}
