using System;
using UnityEngine;

/// <summary>
/// Unity Localization 패키지의 String Table 항목을 참조하기 위한 래퍼 클래스.
/// 실제 텍스트(한국어/영어/기타)는 전부 Localization 테이블에서 관리하고,
/// 이 클래스는 "어떤 테이블의 어떤 키를 쓸지"만 기억한다.
/// </summary>
[Serializable]
public class LocalText
{
    [Tooltip("참조할 String Table 컬렉션 이름. 예: \"UI\", \"Tag\", \"Event\" 등")]
    [SerializeField] private string tableReference;

    [Tooltip("String Table 안에서 사용할 항목 키. 예: \"TITLE_START\", \"TAG_VOCAL_GENIUS\" 등")]
    [SerializeField] private string entryKey;

    /// <summary>
    /// 참조할 String Table 컬렉션 이름.
    /// Unity Localization의 String Table Collection 이름과 일치해야 한다.
    /// </summary>
    public string TableReference => tableReference;

    /// <summary>
    /// String Table 안에서 사용할 항목 키.
    /// </summary>
    public string EntryKey => entryKey;

    /// <summary>
    /// 테이블과 키가 모두 유효하게 설정되어 있는지 여부.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrEmpty(tableReference) &&
        !string.IsNullOrEmpty(entryKey);

    /// <summary>
    /// 코드에서 편하게 생성할 수 있도록 하는 생성자.
    /// (Unity 인스펙터에서는 사용되지 않는다)
    /// </summary>
    public LocalText(string tableReference, string entryKey)
    {
        this.tableReference = tableReference;
        this.entryKey = entryKey;
    }

    /// <summary>
    /// 파라미터 없는 기본 생성자.
    /// Unity 직렬화를 위해 필요하다.
    /// </summary>
    public LocalText()
    {
    }
}