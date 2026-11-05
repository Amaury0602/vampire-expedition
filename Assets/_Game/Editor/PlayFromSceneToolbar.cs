using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Vehicle.Editor
{
    /// <summary>
    /// Bouton dans la toolbar principale (Unity 6.1+ API MainToolbarElement) qui lance le Play mode
    /// depuis une scène configurée, sans changer la scène ouverte dans l'éditeur.
    /// Utilise EditorSceneManager.playModeStartScene, réinitialisé en sortie de Play pour ne pas
    /// détourner le bouton Play normal.
    /// </summary>
    public static class PlayFromSceneToolbar
    {
        private const string OverrodeKey = "PlayFromScene.Overrode";

        [MainToolbarElement("Vehicle/PlayFromScene", defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 1)]
        public static MainToolbarElement CreateButton()
        {
            var content = new MainToolbarContent(
                "▶ Scene",
                "Lance le Play depuis la scène configurée (Tools > Play From Scene > Configure).");
            return new MainToolbarButton(content, OnClicked);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnClicked()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            string scenePath = PlayFromSceneConfig.ScenePath;
            if (string.IsNullOrEmpty(scenePath))
            {
                if (EditorUtility.DisplayDialog("Play From Scene",
                    "Aucune scène de démarrage configurée.", "Configurer", "Annuler"))
                {
                    PlayFromSceneConfigWindow.Open();
                }
                return;
            }

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                Debug.LogError($"[PlayFromScene] Scène introuvable : {scenePath}. Reconfigure via Tools > Play From Scene > Configure.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.playModeStartScene = sceneAsset;
            // SessionState survit au domain reload provoqué par l'entrée en Play (contrairement à un static bool).
            SessionState.SetBool(OverrodeKey, true);
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(OverrodeKey, false))
            {
                EditorSceneManager.playModeStartScene = null;
                SessionState.EraseBool(OverrodeKey);
            }
        }
    }

    /// <summary>
    /// Persistance (par projet) de la scène de démarrage utilisée par le bouton.
    /// </summary>
    internal static class PlayFromSceneConfig
    {
        private const string DefaultScenePath = "Assets/_Game/Scenes/Game.unity";

        private static string Key
        {
            get { return "PlayFromScene.ScenePath." + PlayerSettings.productName; }
        }

        public static string ScenePath
        {
            get { return EditorPrefs.GetString(Key, DefaultScenePath); }
            set { EditorPrefs.SetString(Key, value); }
        }
    }

    /// <summary>
    /// Petite fenêtre pour choisir la scène de démarrage du bouton.
    /// </summary>
    public class PlayFromSceneConfigWindow : EditorWindow
    {
        [MenuItem("Tools/Play From Scene/Configure...")]
        public static void Open()
        {
            var window = GetWindow<PlayFromSceneConfigWindow>(true, "Play From Scene", true);
            window.minSize = new Vector2(380, 90);
            window.maxSize = new Vector2(600, 90);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scène lancée par le bouton toolbar", EditorStyles.boldLabel);

            var current = AssetDatabase.LoadAssetAtPath<SceneAsset>(PlayFromSceneConfig.ScenePath);
            EditorGUI.BeginChangeCheck();
            var selected = (SceneAsset)EditorGUILayout.ObjectField(
                "Start Scene", current, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                PlayFromSceneConfig.ScenePath = selected != null ? AssetDatabase.GetAssetPath(selected) : string.Empty;
            }
        }
    }
}
