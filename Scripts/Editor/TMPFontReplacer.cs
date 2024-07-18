using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TMPro.EditorUtilities
{
    public class TMPFontReplacer : EditorWindow
    {
        [MenuItem("Window/TextMeshPro/Font Replacer")]
        public static void ShowFontReplacerWIndow()
        {
            var window = GetWindow(typeof(TMPFontReplacer));
            window.titleContent = new GUIContent("TMP Font Replacer");
            window.Focus();
        }

        static bool m_IsSelectOldFont = false;
        static string m_SelectedFolderPath;
        static TMP_FontAsset m_OldTextMeshProFont;
        static TMP_FontAsset m_NewTextMeshProFont;

        void OnEnable()
        {
            minSize = new Vector2(350, minSize.y);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            m_IsSelectOldFont = EditorGUILayout.Toggle("Specify Old Font", m_IsSelectOldFont);

            EditorGUI.BeginDisabledGroup(!m_IsSelectOldFont);
            m_OldTextMeshProFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Old TMP Font", m_OldTextMeshProFont, typeof(TMP_FontAsset), true);
            GUILayout.Space(5);
            EditorGUI.EndDisabledGroup();

            m_NewTextMeshProFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New TMP Font", m_NewTextMeshProFont, typeof(TMP_FontAsset), true, GUILayout.MinWidth(100));
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Folder Path", m_SelectedFolderPath);

            if (GUILayout.Button("Select", GUILayout.MaxWidth(80)))
            {
                var paht = EditorUtility.OpenFolderPanel("Select Path", Application.dataPath, "");
                m_SelectedFolderPath = paht.Replace(Application.dataPath, "Assets");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            var isFontEmpty = IsFontEmpty();
            EditorGUI.BeginDisabledGroup(isFontEmpty || Selection.gameObjects.Length == 0);
            if (GUILayout.Button("Replace Selected Prefab"))
            {
                ReplaceFont(Selection.gameObjects.ToList());
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(isFontEmpty || string.IsNullOrEmpty(m_SelectedFolderPath));
            if (GUILayout.Button("Replace Selected Path"))
            {
                ReplaceSelectPathFont();
            }
            EditorGUI.EndDisabledGroup();
        }

        static bool IsFontEmpty()
        {
            return m_IsSelectOldFont && m_OldTextMeshProFont == null || m_NewTextMeshProFont == null;
        }

        public static void ReplaceSelectPathFont()
        {
            var selectedObjects = new List<GameObject>();

            if (!string.IsNullOrEmpty(m_SelectedFolderPath))
            {
                string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { m_SelectedFolderPath });
                foreach (string guid in prefabGUIDs)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                    if (prefab != null)
                    {
                        selectedObjects.Add(prefab);
                    }
                }
            }
            ReplaceFont(selectedObjects);
        }

        public static void ReplaceFont(List<GameObject> targets)
        {
            if (targets == null || targets.Count < 1)
            {
                Debug.LogError("The number of modifiable fonts is empty.");
                return;
            }

            int curCount = 0;
            int maxCount = targets.Count;

            for (int i = 0; i < targets.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Replacing Fonts", "Show a progress bar for the given seconds", curCount / maxCount);
                ReplaceTMPFont(targets[i].GetComponentsInChildren<TMP_Text>(true));
                curCount++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        static void ReplaceTMPFont(TMP_Text[] texts)
        {
            foreach (var text in texts)
            {
                Undo.RecordObject(text, text.gameObject.name);

                if (m_IsSelectOldFont)
                {
                    if (text.font != null && text.font != m_OldTextMeshProFont)
                    {
                        continue;
                    }
                }

                if (m_NewTextMeshProFont != null)
                {
                    text.font = m_NewTextMeshProFont;
                }

                EditorUtility.SetDirty(text);
            }
        }
    }
}