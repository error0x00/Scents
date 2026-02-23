#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Object = UnityEngine.Object;

// EventDatabase 전용 편집 윈도우
// 좌측 이벤트 리스트 / 우측 상세 편집 구조
public class EventDatabaseWindow : OdinEditorWindow
{
    // 기본 에셋 경로
    private const string DefaultDatabaseAssetPath = "Assets/GameData/EventDatabase.asset";
    private const string EventsFolderPath = "Assets/GameData/Events";

    [SerializeField]
    private EventDatabase database;

    private int selectedIndex = -1;
    private Editor currentEventEditor;
    private Vector2 listScroll;
    private Vector2 detailScroll;

    // Unity 상단 메뉴에서 창을 열기 위한 항목 등록
    [MenuItem("GameData/Event Database Window")]
    private static void OpenWindowMenu() => OpenWindow(null);

    // 코드에서 윈도우를 열 때 사용
    public static void OpenWindow(EventDatabase db)
    {
        var window = GetWindow<EventDatabaseWindow>();
        window.titleContent = new GUIContent("Event Database");
        window.minSize = new Vector2(700f, 350f);

        if (db != null)
            window.SetDatabase(db);
        else if (window.database == null)
        {
            var loaded = AssetDatabase.LoadAssetAtPath<EventDatabase>(DefaultDatabaseAssetPath);
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
    private void SetDatabase(EventDatabase db)
    {
        database = db;
        selectedIndex = -1;
        CreateEventEditor(null);
        Repaint();
    }

    // Database가 없을 때 보여주는 UI
    private void DrawDatabasePickerUI()
    {
        EditorGUILayout.HelpBox(
            "EventDatabase 에셋이 지정되지 않았습니다.\n" +
            "아래 버튼을 눌러 기본 경로에서 찾거나 새로 생성할 수 있습니다.",
            MessageType.Info);

        if (GUILayout.Button("Find or Create EventDatabase.asset"))
        {
            database = GetOrCreateDatabase();
            selectedIndex = -1;
            CreateEventEditor(null);
        }
    }

    // 상단 툴바: DB 정보, 이벤트 수, 재빌드 버튼
    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            string dbLabel = database != null
                ? $"Database: {database.name} (Events: {database.Count})"
                : "Database: (None)";

            GUILayout.Label(dbLabel, EditorStyles.miniBoldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rebuild From GameData/Events", EditorStyles.toolbarButton))
            {
                EventDatabaseEditor.RebuildFromFolder(database, EventsFolderPath);
                selectedIndex = -1;
                CreateEventEditor(null);
            }
        }
    }

    // 좌측 리스트 / 우측 상세 편집 레이아웃
    private void DrawSplitView()
    {
        var list = database.Events;
        int count = list != null ? list.Count : 0;

        using (new EditorGUILayout.HorizontalScope())
        {
            // 좌측: 이벤트 리스트
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(250f)))
            {
                GUILayout.Label("Events", EditorStyles.boldLabel);

                listScroll = EditorGUILayout.BeginScrollView(listScroll, "box");
                if (count == 0)
                {
                    GUILayout.Label("No events in database.", EditorStyles.miniLabel);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        var ev = list[i];
                        string label = ev != null ? ((Object)ev).name : "(null)";

                        bool isSelected = (i == selectedIndex);
                        GUIStyle style = isSelected
                            ? EditorStyles.miniButtonMid
                            : EditorStyles.miniButton;

                        if (GUILayout.Button(label, style))
                        {
                            if (selectedIndex != i)
                            {
                                selectedIndex = i;
                                CreateEventEditor(ev);
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            // 우측: 선택된 이벤트 상세 편집
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                GUILayout.Label("Event Detail", EditorStyles.boldLabel);

                detailScroll = EditorGUILayout.BeginScrollView(detailScroll, "box");

                if (selectedIndex < 0 || selectedIndex >= count)
                {
                    GUILayout.Label("왼쪽 리스트에서 이벤트를 선택하세요.", EditorStyles.miniLabel);
                }
                else
                {
                    if (currentEventEditor == null)
                        CreateEventEditor(list[selectedIndex]);

                    if (currentEventEditor != null)
                        currentEventEditor.OnInspectorGUI();
                    else
                        GUILayout.Label("선택한 이벤트를 표시할 수 없습니다.", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }

    // 선택된 EventData에 대한 에디터 인스턴스 생성
    private void CreateEventEditor(Object target)
    {
        if (currentEventEditor != null)
        {
            DestroyImmediate(currentEventEditor);
            currentEventEditor = null;
        }

        if (target != null)
            currentEventEditor = Editor.CreateEditor(target);
    }

    // EventDatabase 에셋을 기본 경로에서 찾거나 새로 생성
    private static EventDatabase GetOrCreateDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<EventDatabase>(DefaultDatabaseAssetPath);
        if (db != null)
            return db;

        db = ScriptableObject.CreateInstance<EventDatabase>();

        var folder = "Assets/GameData";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "GameData");

        AssetDatabase.CreateAsset(db, DefaultDatabaseAssetPath);
        AssetDatabase.SaveAssets();

        return db;
    }
}

#endif