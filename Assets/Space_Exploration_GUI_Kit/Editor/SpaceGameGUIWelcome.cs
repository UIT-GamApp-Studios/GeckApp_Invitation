using UnityEditor;
using UnityEngine;



namespace Danprops.SpaceGameGUIKit
{
    [InitializeOnLoad]
    public class SpaceGameGUIWelcome : EditorWindow
    {
        // Bump this version string on each update to re-show the window to existing users
        private const string Version = "1.1";
        private const string PrefKey = "danprops.SpaceGameGUI.WelcomeShown." + Version;
        private const string ReviewUrl =
            "https://assetstore.unity.com/packages/2d/gui/space-game-gui-kit-298577#reviews";

        static SpaceGameGUIWelcome()
        {
            EditorApplication.delayCall += ShowOnce;
        }

        private static void ShowOnce()
        {
            if (EditorPrefs.GetBool(PrefKey, false))
                return;

            EditorPrefs.SetBool(PrefKey, true);
            ShowWindow();
        }


        // popup menu
        [MenuItem("Window/Space Game GUI Kit/Welcome")]
        private static void ShowWindow()
        {
            var window = GetWindow<SpaceGameGUIWelcome>(true, "Space Game GUI Kit", true);
            window.minSize = new Vector2(420, 280);
            window.maxSize = new Vector2(420, 280);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(12);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Thanks for installing the Space Game GUI Kit", titleStyle);

            EditorGUILayout.Space(8);

            var bodyStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 12,
                richText = true
            };
            EditorGUILayout.LabelField(
                "This kit is free and was built solo. A review is the single biggest thing that " +
                "helps my creations gain visibility and it tells me what to work on next.\n\n" +
                "Leave a review if you enjoyed the pack or what you'd want added and I'll " +
                "consider those requests in the next update!",
                bodyStyle, GUILayout.Height(96));

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Leave a review (opens browser)", GUILayout.Height(34)))
            {
                Application.OpenURL(ReviewUrl);
                Close();
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Request a feature", GUILayout.Height(24)))
                {
                    Application.OpenURL(ReviewUrl);
                }

                if (GUILayout.Button("Maybe later", GUILayout.Height(24)))
                {
                    Close();
                }
            }

            EditorGUILayout.Space(6);

            var footStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            EditorGUILayout.LabelField(
                "Reopen anytime: Window => Space Game GUI Kit => Welcome",
                footStyle);
        }
    }
}