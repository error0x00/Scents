using UnityEngine;
using Sirenix.OdinInspector;

// 향료 한 종류의 데이터를 정의하는 ScriptableObject
[CreateAssetMenu(menuName = "Game/Material", fileName = "mat_")]
public class MatData : ScriptableObject
{
    [BoxGroup("기본 정보")]
    public string id;

    [BoxGroup("기본 정보")]
    public LocalText matName;

    [BoxGroup("기본 정보")]
    [LabelText("표시 이름 (임시)")]
    public string displayName;

    [BoxGroup("분류")]
    public NotePosition note;

    [BoxGroup("분류")]
    public ScentFamily family;

    [BoxGroup("분류")]
    public MaterialType materialType;

    // 재고 단위: 방울(drop). 1병 = 10방울
    [BoxGroup("재고")]
    [MinValue(0)]
    public int stock;
}