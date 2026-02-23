using System.Collections.Generic;
using UnityEngine;

// 프로젝트 전체에서 사용할 ScentTag 에셋을 모아두는 데이터베이스
// 런타임/에디터 코드에서 id로 ScentTag를 조회할 수 있게 해준다

[CreateAssetMenu(fileName = "ScentTagDatabase", menuName = "GameData/ScentTag Database")]
public class ScentTagDatabase : ScriptableObject
{
    [SerializeField]
    private List<ScentTag> tags = new List<ScentTag>();

    public IReadOnlyList<ScentTag> Tags => tags;

    private Dictionary<string, ScentTag> _tagById;

    public int Count => tags != null ? tags.Count : 0;

    private void OnEnable() => RebuildCache();
    private void OnValidate() => RebuildCache();

    private void RebuildCache()
    {
        if (tags == null || tags.Count == 0)
        {
            _tagById = null;
            return;
        }

        if (_tagById == null)
            _tagById = new Dictionary<string, ScentTag>();
        else
            _tagById.Clear();

        foreach (var tag in tags)
        {
            if (tag == null || string.IsNullOrEmpty(tag.id))
                continue;

#if UNITY_EDITOR
            if (_tagById.ContainsKey(tag.id))
            {
                Debug.LogWarning($"[ScentTagDatabase] Duplicate id: '{tag.id}'", this);
                continue;
            }
#endif
            _tagById[tag.id] = tag;
        }
    }

    public ScentTag GetById(string id)
    {
        if (string.IsNullOrEmpty(id) || _tagById == null)
            return null;

        _tagById.TryGetValue(id, out var result);
        return result;
    }

    public bool TryGetById(string id, out ScentTag result)
    {
        result = null;
        if (string.IsNullOrEmpty(id) || _tagById == null)
            return false;

        return _tagById.TryGetValue(id, out result);
    }

    public bool ContainsId(string id)
    {
        return !string.IsNullOrEmpty(id) && _tagById != null && _tagById.ContainsKey(id);
    }
}