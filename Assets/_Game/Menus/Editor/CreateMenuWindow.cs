using UnityEditor;
using UnityEngine;

namespace Menus.Editor
{
    /// <summary>
    /// Popup "Menus > Create New Menu" : demande un nom et scaffolde un menu complet
    /// (dossier + script héritant de Menu + prefab Canvas, puis enregistrement au MenuDatabase).
    /// </summary>
    public class CreateMenuWindow : EditorWindow
    {
        private string _name = "NewMenu";

        [MenuItem("Menus/Create New Menu")]
        private static void Open()
        {
            var window = GetWindow<CreateMenuWindow>(true, "Create New Menu", true);
            window.minSize = new Vector2(340, 120);
            window.maxSize = new Vector2(600, 120);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Nom du menu", EditorStyles.boldLabel);
            _name = EditorGUILayout.TextField("Name", _name);

            bool valid = MenuScaffolder.IsValidMenuName(_name, out string error);
            if (!valid)
                EditorGUILayout.HelpBox(error, MessageType.Warning);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!valid))
            {
                if (GUILayout.Button("Create", GUILayout.Height(28)))
                {
                    MenuScaffolder.CreateMenu(_name);
                    Close();
                }
            }
        }
    }
}
