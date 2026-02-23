using UnityEngine;

// 이벤트 조건 및 진도 관리용 태그
// 향수/재료 분류용 ScentTag와 별개로 서사 시스템에서만 사용

[CreateAssetMenu(menuName = "Game/EventTag", fileName = "etag_")]
public class EventTag : ScriptableObject
{
    public string id;
    public LocalText desc;
}