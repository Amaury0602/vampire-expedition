using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Editor
{
    /// <summary>
    /// Logique de scaffolding des menus : création dossier + script + prefab configuré,
    /// enregistrement dans le MenuDatabase, et renommage.
    /// La création est en deux phases (le type C# n'existe qu'après recompilation) :
    /// phase 1 écrit le script + mémorise le nom dans SessionState, phase 2 (post-reload)
    /// construit le prefab.
    /// </summary>
    public static class MenuScaffolder
    {
        public const string MenusRoot = "Assets/_Game/Menus";
        private const string PendingKey = "MenuScaffolder.PendingMenuName";

        private static readonly Regex IdentifierRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$");

        private static readonly HashSet<string> CSharpKeywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while"
        };

        // ---- Validation ---------------------------------------------------

        public static bool IsValidMenuName(string name, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Le nom ne peut pas être vide.";
                return false;
            }

            if (!IdentifierRegex.IsMatch(name))
            {
                error = "Nom invalide : lettres/chiffres/_ uniquement, ne commence pas par un chiffre.";
                return false;
            }

            if (CSharpKeywords.Contains(name))
            {
                error = $"'{name}' est un mot-clé C# réservé.";
                return false;
            }

            if (AssetDatabase.IsValidFolder($"{MenusRoot}/{name}"))
            {
                error = $"Un menu '{name}' existe déjà.";
                return false;
            }

            return true;
        }

        // ---- Phase 1 : script + état pending ------------------------------

        public static void CreateMenu(string name)
        {
            string folder = $"{MenusRoot}/{name}";
            AssetDatabase.CreateFolder(MenusRoot, name);

            string scriptPath = $"{folder}/{name}.cs";
            File.WriteAllText(scriptPath, ScriptTemplate(name));

            SessionState.SetString(PendingKey, name);
            AssetDatabase.ImportAsset(scriptPath);
            AssetDatabase.Refresh();
        }

        private static string ScriptTemplate(string name)
        {
            return $@"using UnityEngine;

namespace Menus
{{
    public class {name} : Menu
    {{
    }}
}}
";
        }

        // ---- Phase 2 : construction du prefab après recompile -------------

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            string name = SessionState.GetString(PendingKey, string.Empty);
            if (string.IsNullOrEmpty(name))
                return;

            SessionState.EraseString(PendingKey);
            BuildPrefab(name);
        }

        private static void BuildPrefab(string name)
        {
            Type type = FindMenuType(name);
            if (type == null)
            {
                Debug.LogError($"[MenuScaffolder] Type 'Menus.{name}' introuvable après compilation. Prefab non créé.");
                return;
            }

            var go = new GameObject(name) { layer = LayerMask.NameToLayer("UI") };
            try
            {
                var canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f;

                go.AddComponent<GraphicRaycaster>();
                go.AddComponent(type);

                string prefabPath = $"{MenusRoot}/{name}/{name}.prefab";
                GameObject prefabRoot = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

                // Récupérer le composant Menu directement sur le prefab sauvegardé :
                // LoadAssetAtPath<Menu>(path) renvoie souvent null (le main asset est le GameObject).
                var menuComp = prefabRoot != null ? prefabRoot.GetComponent<Menu>() : null;
                RegisterInDatabase(menuComp);

                Debug.Log($"[MenuScaffolder] Menu '{name}' créé : {prefabPath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static Type FindMenuType(string name)
        {
            string fullName = $"Menus.{name}";
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName, false))
                .FirstOrDefault(t => t != null && typeof(Menu).IsAssignableFrom(t));
        }

        // ---- Enregistrement dans le MenuDatabase --------------------------

        private static void RegisterInDatabase(Menu menuComp)
        {
            if (menuComp == null)
            {
                Debug.LogWarning("[MenuScaffolder] Composant Menu introuvable sur le prefab — non enregistré.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:MenuDatabase");
            if (guids.Length == 0)
            {
                Debug.LogWarning("[MenuScaffolder] Aucun MenuDatabase trouvé — menu non enregistré.");
                return;
            }
            if (guids.Length > 1)
            {
                Debug.LogWarning("[MenuScaffolder] Plusieurs MenuDatabase trouvés — menu non enregistré (ambigu).");
                return;
            }

            string dbPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var database = AssetDatabase.LoadAssetAtPath<MenuDatabase>(dbPath);
            if (database == null)
            {
                Debug.LogWarning("[MenuScaffolder] Chargement du MenuDatabase échoué.");
                return;
            }

            var so = new SerializedObject(database);
            SerializedProperty list = so.FindProperty("_menuPrefabs");

            // Éviter un doublon si déjà présent.
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == menuComp)
                    return;
            }

            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = menuComp;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }

        // ---- Rename (single-phase) ----------------------------------------

        public static void RenameMenu(string oldName, string newName)
        {
            if (!IsValidMenuName(newName, out string error))
            {
                Debug.LogError($"[MenuScaffolder] Rename annulé : {error}");
                return;
            }

            string oldFolder = $"{MenusRoot}/{oldName}";
            string scriptPath = $"{oldFolder}/{oldName}.cs";
            string prefabPath = $"{oldFolder}/{oldName}.prefab";

            AssetDatabase.StartAssetEditing();
            try
            {
                // 1. Renommer le script (GUID préservé) puis réécrire le nom de classe.
                AssetDatabase.RenameAsset(scriptPath, newName);
                string newScriptPath = $"{oldFolder}/{newName}.cs";
                string source = File.ReadAllText(newScriptPath);
                source = Regex.Replace(source, $@"\bclass\s+{Regex.Escape(oldName)}\b", $"class {newName}");
                File.WriteAllText(newScriptPath, source);

                // 2. Renommer le prefab (lien m_Script conservé par GUID).
                AssetDatabase.RenameAsset(prefabPath, newName);

                // 3. Renommer le dossier.
                AssetDatabase.RenameAsset(oldFolder, newName);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            Debug.Log($"[MenuScaffolder] Menu '{oldName}' renommé en '{newName}'. " +
                      "⚠ Les références code (MenuManager.Show<" + oldName + ">(), Get<" + oldName + ">()) ne sont PAS réécrites.");
        }

        // ---- Découverte des menus existants -------------------------------

        public static string[] GetExistingMenuNames()
        {
            if (!AssetDatabase.IsValidFolder(MenusRoot))
                return Array.Empty<string>();

            return AssetDatabase.GetSubFolders(MenusRoot)
                .Select(Path.GetFileName)
                .Where(dir => dir != "Editor"
                              && File.Exists($"{MenusRoot}/{dir}/{dir}.cs")
                              && File.Exists($"{MenusRoot}/{dir}/{dir}.prefab"))
                .OrderBy(n => n)
                .ToArray();
        }
    }
}
