using System;
using System.Collections.Generic;
using UnityEngine;

// 이벤트 발생 조건 정의
[Serializable]
public class EventCondition
{
    public int minWeek;
    public int maxWeek;

    // 이벤트 발생에 필요한 EventTag 목록
    public List<EventTag> needTags;

    // 이벤트 발생을 막는 EventTag 목록
    public List<EventTag> banTags;

    // 한 번만 발생하는 이벤트 여부
    public bool onlyOnce;
}

// 이벤트 선택지 선택 후 적용되는 결과
[Serializable]
public class EventResult
{
    public int deltaGold;
    public int deltaKarma;

    // 결과로 추가되는 EventTag 목록
    public List<EventTag> addTags;

    // 결과로 제거되는 EventTag 목록
    public List<EventTag> removeTags;
}

// 이벤트 선택지 정의
[Serializable]
public class EventChoice
{
    public LocalText text;

    // 이 선택지 선택 후 이어지는 이벤트 ID
    public string nextEventId;
    public EventResult result;
}

// 이벤트 데이터 정의
[CreateAssetMenu(menuName = "Game/Event", fileName = "ev_")]
public class EventData : ScriptableObject
{
    public string eventId;
    public EventType type;
    public LocalText summary;
    public LocalText mainText;
    public EventCondition condition;
    public List<EventChoice> choices;

    // 랜덤 선택 시 사용할 가중치
    public float weight = 1f;
}