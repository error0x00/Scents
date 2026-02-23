using System.Collections.Generic;
using UnityEngine;

// 프로젝트 전체에서 사용할 EventData 에셋을 모아두는 데이터베이스
// 런타임/에디터 코드에서 eventId로 EventData를 조회할 수 있게 해준다
[CreateAssetMenu(fileName = "EventDatabase", menuName = "GameData/Event Database")]
public class EventDatabase : ScriptableObject
{
    [SerializeField]
    private List<EventData> events = new List<EventData>();

    public IReadOnlyList<EventData> Events => events;

    // eventId로 빠르게 찾기 위한 캐시
    private Dictionary<string, EventData> _eventById;

    public int Count => events != null ? events.Count : 0;

    private void OnEnable() => RebuildCache();
    private void OnValidate() => RebuildCache();

    // eventId 기반 캐시 재구성
    private void RebuildCache()
    {
        if (events == null)
        {
            _eventById = null;
            return;
        }

        if (_eventById == null)
            _eventById = new Dictionary<string, EventData>();
        else
            _eventById.Clear();

        foreach (var ev in events)
        {
            if (ev == null || string.IsNullOrEmpty(ev.eventId))
                continue;

            if (_eventById.ContainsKey(ev.eventId))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[EventDatabase] Duplicate eventId: {ev.eventId}", this);
#endif
                continue;
            }

            _eventById.Add(ev.eventId, ev);
        }
    }

    // eventId로 EventData 검색, 없으면 null 반환
    public EventData GetById(string eventId)
    {
        if (string.IsNullOrEmpty(eventId) || _eventById == null)
            return null;

        _eventById.TryGetValue(eventId, out var result);
        return result;
    }

    // eventId로 EventData 검색, 성공 여부를 bool로 반환
    public bool TryGetById(string eventId, out EventData result)
    {
        result = null;
        if (string.IsNullOrEmpty(eventId) || _eventById == null)
            return false;

        return _eventById.TryGetValue(eventId, out result);
    }
}