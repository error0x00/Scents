using System;
using System.Collections.Generic;

// 전체 게임 플레이 동안 발생한 이벤트 히스토리를 보관하는 클래스
[Serializable]
public class GameEventHistory
{
    // 이벤트의 누적 발생 횟수
    public Dictionary<EventData, int> eventHistory =
        new Dictionary<EventData, int>();

    // 이벤트가 마지막으로 발생한 절대 일자 (worldDay 기준)
    public Dictionary<EventData, int> eventLastDay =
        new Dictionary<EventData, int>();

    // 이벤트 발생 횟수 증가
    public void Increment(EventData ev, int worldDay)
    {
        if (ev == null)
            return;

        if (!eventHistory.ContainsKey(ev))
            eventHistory[ev] = 0;

        eventHistory[ev]++;

        eventLastDay[ev] = worldDay;
    }

    // 이벤트가 몇 번 발생했는지 반환
    public int GetCount(EventData ev)
    {
        if (ev == null)
            return 0;

        if (eventHistory.TryGetValue(ev, out int count))
            return count;

        return 0;
    }

    // 마지막 발생일 반환
    public int GetLastDay(EventData ev)
    {
        if (ev == null)
            return -1;

        if (eventLastDay.TryGetValue(ev, out int day))
            return day;

        return -1;
    }
}
