#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// EventDatabase 인스펙터 확장
// GameData/Events 폴더 스캔 및 자동 등록 기능 제공
[CustomEditor(typeof(EventDatabase))]
public class EventDatabaseEditor : Editor
{
    // EventData 에셋을 찾을 폴더 경로
    private const string EventsFolderPath = "Assets/GameData/Events";

    // 자동 생성 시 사용할 Database 에셋 경로
    private const string DefaultDatabaseAssetPath = "Assets/GameData/EventDatabase.asset";

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 출력
        DrawDefaultInspector();

        EditorGUILayout.Space();

        var db = (EventDatabase)target;

        if (GUILayout.Button("Open Event Database Window"))
            EventDatabaseWindow.OpenWindow(db);

        if (GUILayout.Button("Rebuild From GameData/Events"))
            RebuildFromFolder(db, EventsFolderPath);
    }

    // 메뉴에서 직접 호출할 수 있는 자동 빌드 기능
    // EventDatabase 에셋이 없으면 새로 만들고, 있으면 재사용
    [MenuItem("GameData/Rebuild Event Database (auto)")]
    private static void RebuildOrCreateMenu()
    {
        var db = AssetDatabase.LoadAssetAtPath<EventDatabase>(DefaultDatabaseAssetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<EventDatabase>();

            var folder = "Assets/GameData";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets", "GameData");

            AssetDatabase.CreateAsset(db, DefaultDatabaseAssetPath);
            AssetDatabase.SaveAssets();
        }

        RebuildFromFolder(db, EventsFolderPath);

        // 에셋 선택해서 바로 인스펙터에서 보이게
        Selection.activeObject = db;
    }

    // 지정된 폴더에서 모든 EventData 에셋을 찾아 EventDatabase를 재구성
    public static void RebuildFromFolder(EventDatabase db, string folderPath)
    {
        if (db == null) return;

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"[EventDatabaseEditor] Folder not found: {folderPath}");
            return;
        }

        // GameData/Events 폴더에서 EventData 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:EventData", new[] { folderPath });
        var eventList = new List<EventData>();

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var ev = AssetDatabase.LoadAssetAtPath<EventData>(assetPath);
            if (ev != null)
                eventList.Add(ev);
        }

        // SerializedObject를 사용해 private 필드(events)에 값 넣기
        var so = new SerializedObject(db);
        var eventsProp = so.FindProperty("events");

        eventsProp.ClearArray();

        for (int i = 0; i < eventList.Count; i++)
        {
            eventsProp.InsertArrayElementAtIndex(i);
            eventsProp.GetArrayElementAtIndex(i).objectReferenceValue = eventList[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);

        // 캐시 재구성을 위해 수동으로 OnValidate 호출
        db.GetType()
          .GetMethod("OnValidate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          ?.Invoke(db, null);
    }
}

#endif