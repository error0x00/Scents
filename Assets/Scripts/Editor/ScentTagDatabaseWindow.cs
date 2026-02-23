#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Object = UnityEngine.Object;

// ScentTagDatabase 전용 편집 윈도우
// 좌측 태그 리스트 / 우측 상세 편집 구조
public class ScentTagDatabaseWindow : OdinEditorWindow
{
    // 기본 에셋 경로
    private const string DefaultDatabaseAssetPath = "Assets/GameData/ScentTagDatabase.asset";
    private const string TagsFolderPath = "Assets/GameData/Tags";

    [SerializeField]
    private ScentTagDatabase database;

    private int selectedIndex = -1;
    private Editor currentTagEditor;
    private Vector2 listScroll;
    private Vector2 detailScroll;

    // Unity 상단 메뉴에서 창을 열기 위한 항목 등록
    [MenuItem("GameData/ScentTag Database Window")]
    private static void OpenWindowMenu() => OpenWindow(null);

    // 코드에서 윈도우를 열 때 사용
    public static void OpenWindow(ScentTagDatabase db)
    {
        var window = GetWindow<ScentTagDatabaseWindow>();
        window.titleContent = new GUIContent("ScentTag Database");
        window.minSize = new Vector2(700f, 350f);

        if (db != null)
            window.SetDatabase(db);
        else if (window.database == null)
        {
            var loaded = AssetDatabase.LoadAssetAtPath<ScentTagDatabase>(DefaultDatabaseAssetPath);
            if (loaded != null)
                window.SetDatabase(loaded);
        }

        window.Show();
    }

    // OdinEditorWindow IMGUI 렌더링 진입점
    protected override void OnImGUI()
    {
        EditorGUILayout.Space();

        if (database == null)
        {
            DrawDatabasePickerUI();
            return;
        }

        DrawToolbar();
        EditorGUILayout.Space();
        DrawSplitView();
    }

    // 현재 윈도우에서 사용할 데이터베이스 설정
    private void SetDatabase(ScentTagDatabase db)
    {
        database = db;
        selectedIndex = -1;
        CreateTagEditor(null);
        Repaint();
    }

    // Database가 없을 때 보여주는 UI
    private void DrawDatabasePickerUI()
    {
        EditorGUILayout.HelpBox(
            "ScentTagDatabase 에셋이 지정되지 않았습니다.\n" +
            "아래 버튼을 눌러 기본 경로에서 찾거나 새로 생성할 수 있습니다.",
            MessageType.Info);

        if (GUILayout.Button("Find or Create ScentTagDatabase.asset"))
        {
            database = GetOrCreateDatabase();
            selectedIndex = -1;
            CreateTagEditor(null);
        }
    }

    // 상단 툴바: DB 정보, 태그 수, 재빌드 버튼
    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            string dbLabel = database != null
                ? $"Database: {database.name} (Tags: {database.Count})"
                : "Database: (None)";

            GUILayout.Label(dbLabel, EditorStyles.miniBoldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rebuild From GameData/Tags", EditorStyles.toolbarButton))
            {
                ScentTagDatabaseEditor.RebuildFromFolder(database, TagsFolderPath);
                selectedIndex = -1;
                CreateTagEditor(null);
            }
        }
    }

    // 좌측 리스트 / 우측 상세 편집 레이아웃
    private void DrawSplitView()
    {
        var tags = database != null ? database.Tags : null;
        int tagCount = tags != null ? tags.Count : 0;

        using (new EditorGUILayout.HorizontalScope())
        {
            // 좌측: 태그 리스트
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(250f)))
            {
                GUILayout.Label("ScentTags", EditorStyles.boldLabel);

                listScroll = EditorGUILayout.BeginScrollView(listScroll, "box");
                if (tagCount == 0)
                {
                    GUILayout.Label("No tags in database.", EditorStyles.miniLabel);
                }
                else
                {
                    for (int i = 0; i < tagCount; i++)
                    {
                        var tag = tags[i];
                        string label = tag != null ? ((Object)tag).name : "(null)";

                        bool isSelected = (i == selectedIndex);
                        GUIStyle style = isSelected
                            ? EditorStyles.miniButtonMid
                            : EditorStyles.miniButton;

                        if (GUILayout.Button(label, style))
                        {
                            if (selectedIndex != i)
                            {
                                selectedIndex = i;
                                CreateTagEditor(tag as Object);
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            // 우측: 선택된 태그 상세 편집
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                GUILayout.Label("Tag Detail", EditorStyles.boldLabel);

                detailScroll = EditorGUILayout.BeginScrollView(detailScroll, "box");

                if (selectedIndex < 0 || selectedIndex >= tagCount)
                {
                    GUILayout.Label("왼쪽 리스트에서 태그를 선택하세요.", EditorStyles.miniLabel);
                }
                else
                {
                    if (currentTagEditor == null)
                        CreateTagEditor(tags[selectedIndex] as Object);

                    if (currentTagEditor != null)
                        currentTagEditor.OnInspectorGUI();
                    else
                        GUILayout.Label("선택한 태그를 표시할 수 없습니다.", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }

    // 선택된 ScentTag에 대한 에디터 인스턴스 생성
    private void CreateTagEditor(Object target)
    {
        if (currentTagEditor != null)
        {
            DestroyImmediate(currentTagEditor);
            currentTagEditor = null;
        }

        if (target != null)
            currentTagEditor = Editor.CreateEditor(target);
    }

    // ScentTagDatabase 에셋을 기본 경로에서 찾거나 새로 생성
    private static ScentTagDatabase GetOrCreateDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<ScentTagDatabase>(DefaultDatabaseAssetPath);
        if (db != null)
            return db;

        db = ScriptableObject.CreateInstance<ScentTagDatabase>();

        var folder = "Assets/GameData";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "GameData");

        AssetDatabase.CreateAsset(db, DefaultDatabaseAssetPath);
        AssetDatabase.SaveAssets();

        return db;
    }
}

#endif