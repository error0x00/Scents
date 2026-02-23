using UnityEngine;

// 향료(재료) 및 향수(완성품) 분류용 태그
// 이벤트 진도 관리용 EventTag와 별개로 향수 시스템에서만 사용

[CreateAssetMenu(menuName = "Game/ScentTag", fileName = "stag_")]
public class ScentTag : ScriptableObject
{
    public string id;
    public ScentTagType type;
    public NotePosition notePosition;
    public ScentFamily family;
    public ScentGender gender;
    public LocalText scentName;
    public LocalText desc;
}