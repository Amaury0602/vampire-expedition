using UnityEditor;
using UnityEngine;

namespace Menus.Editor
{
    /// <summary>
    /// Popup "Menus > Rename Menu" : choisit un menu existant (dropdown) et le renomme
    /// (dossier + script + nom de classe + prefab). Les références code ne sont pas réécrites.
    /// </summary>
    public class RenameMenuWindow : EditorWindow
    {
        private string[] _existingMenus;
        private int _selectedIndex;
        private string _newName = "";

        [MenuItem("Menus/Rename Menu")]
        private static void Open()
        {
            var window = GetWindow<RenameMenuWindow>(true, "Rename Menu", true);
            window.minSize = new Vector2(360, 160);
            window.maxSize = new Vector2(600, 160);
            window.ShowUtility();
        }

        private void OnEnable()
        {
            _existingMenus = MenuScaffolder.GetExistingMenuNames();
        }

        private void OnGUI()
        {
            if (_existingMenus == null || _existingMenus.Length == 0)
            {
                EditorGUILayout.HelpBox("Aucun menu à renommer.", MessageType.Info);
                return;
            }

            _selectedIndex = EditorGUILayout.Popup("Menu", _selectedIndex, _existingMenus);
            _newName = EditorGUILayout.TextField("Nouveau nom", _newName);

            bool valid = MenuScaffolder.IsValidMenuName(_newName, out string error);
            if (!valid)
                EditorGUILayout.HelpBox(error, MessageType.Warning);

            EditorGUILayout.HelpBox(
                "Les références code (MenuManager.Show<...>(), Get<...>()) ne sont PAS réécrites " +
                "et devront être corrigées à la main.",
                MessageType.Warning);

            using (new EditorGUI.DisabledScope(!valid))
            {
                if (GUILayout.Button("Rename", GUILayout.Height(28)))
                {
                    MenuScaffolder.RenameMenu(_existingMenus[_selectedIndex], _newName);
                    Close();
                }
            }
        }
    }
}
