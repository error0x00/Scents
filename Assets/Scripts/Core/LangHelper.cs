using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// Unity Localization 시스템을 기반으로 LocalText 데이터에서 실제 문자열을 얻어오는 헬퍼.
/// LocalText에는 "테이블명"과 "항목키"만 저장되어 있으며,
/// 실제 텍스트는 Localization 패키지의 String Table에서 가져온다.
/// </summary>
public static class LangHelper
{
    /// <summary>
    /// LocalText 내부의 tableReference + entryKey 조합을 이용하여
    /// Localization String Table에서 실제 문자열을 가져온다.
    /// </summary>
    public static string Get(LocalText localText)
    {
        if (localText == null || !localText.IsValid)
            return string.Empty;

        // Unity Localization의 StringDatabase를 통해 문자열 로드
        // 문자열 오버로드 사용: (tableName, entryName)
        return LocalizationSettings.StringDatabase.GetLocalizedString(
            localText.TableReference,
            localText.EntryKey
        );
    }

    /// <summary>
    /// 현재 선택된 Locale을 변경한다.
    /// 예: new LocaleIdentifier("ko"), new LocaleIdentifier("en") 등.
    /// </summary>
    public static void SetLocale(LocaleIdentifier localeId)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeId);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Debug.LogWarning($"[LangHelper] Locale '{localeId}' not found in AvailableLocales.");
        }
    }

    /// <summary>
    /// 현재 활성 Locale 반환 (예: "ko-KR", "en-US")
    /// </summary>
    public static LocaleIdentifier GetCurrentLocale()
    {
        return LocalizationSettings.SelectedLocale.Identifier;
    }
}
