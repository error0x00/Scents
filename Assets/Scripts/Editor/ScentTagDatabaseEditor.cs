#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;

// ScentTagDatabase 인스펙터 확장
// GameData/Tags 폴더 스캔 및 자동 등록 기능 제공
[CustomEditor(typeof(ScentTagDatabase))]
public class ScentTagDatabaseEditor : OdinEditor
{
    // ScentTag 에셋을 찾을 폴더 경로
    private const string TagsFolderPath = "Assets/GameData/Tags";

    // 자동 생성 시 사용할 Database 에셋 경로
    private const string DefaultDatabaseAssetPath = "Assets/GameData/ScentTagDatabase.asset";

    public override void OnInspectorGUI()
    {
        // Odin 기반 기본 인스펙터 출력
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        var db = (ScentTagDatabase)target;

        if (GUILayout.Button("Open ScentTag Database Window"))
            ScentTagDatabaseWindow.OpenWindow(db);

        if (GUILayout.Button("Rebuild From GameData/Tags"))
            RebuildFromFolder(db, TagsFolderPath);
    }

    // 메뉴에서 직접 호출할 수 있는 자동 빌드 기능
    // ScentTagDatabase 에셋이 없으면 새로 만들고, 있으면 재사용
    [MenuItem("GameData/Rebuild ScentTag Database (auto)")]
    private static void RebuildOrCreateMenu()
    {
        var db = AssetDatabase.LoadAssetAtPath<ScentTagDatabase>(DefaultDatabaseAssetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<ScentTagDatabase>();

            var folder = "Assets/GameData";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets", "GameData");

            AssetDatabase.CreateAsset(db, DefaultDatabaseAssetPath);
            AssetDatabase.SaveAssets();
        }

        RebuildFromFolder(db, TagsFolderPath);

        // 에셋 선택해서 바로 인스펙터에서 보이게
        Selection.activeObject = db;
    }

    // 지정된 폴더에서 모든 ScentTag 에셋을 찾아 ScentTagDatabase를 재구성
    public static void RebuildFromFolder(ScentTagDatabase db, string folderPath)
    {
        if (db == null) return;

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"[ScentTagDatabaseEditor] Folder not found: {folderPath}");
            return;
        }

        // GameData/Tags 폴더에서 ScentTag 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:ScentTag", new[] { folderPath });
        var tagList = new List<ScentTag>();

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var tag = AssetDatabase.LoadAssetAtPath<ScentTag>(assetPath);
            if (tag != null)
                tagList.Add(tag);
        }

        // SerializedObject를 사용해 private 필드(tags)에 값 넣기
        var so = new SerializedObject(db);
        var tagsProp = so.FindProperty("tags");

        tagsProp.ClearArray();

        for (int i = 0; i < tagList.Count; i++)
        {
            tagsProp.InsertArrayElementAtIndex(i);
            tagsProp.GetArrayElementAtIndex(i).objectReferenceValue = tagList[i];
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