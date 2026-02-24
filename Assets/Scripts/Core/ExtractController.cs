using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

// 추출 공정 총괄: 보정값 계산, 품질 결정, 재고 반영
public class ExtractController : MonoBehaviour
{
    private const int BaseDrop = 10;

#pragma warning disable 0414
    [BoxGroup("다듬기")]
    [DisplayAsString, HideLabel]
    [SerializeField] private string trimNote = "기본값 1.0배 기준으로 증감";
#pragma warning restore 0414

    [BoxGroup("다듬기 - Root")]
    [InfoBox("칼질 횟수 기준")]
    [LabelText("최소값")]
    [SerializeField] private int rootMin = 5;

    [BoxGroup("다듬기 - Root")]
    [LabelText("기본값")]
    [SerializeField] private int rootOptimal = 10;

    [BoxGroup("다듬기 - Root")]
    [LabelText("최대값")]
    [SerializeField] private int rootMax = 15;

    [BoxGroup("다듬기 - Wood")]
    [InfoBox("360도 회전마다 1회 카운트. 최소 횟수 미충족 시 Low 고정.")]
    [LabelText("최소 충족 횟수")]
    [SerializeField] private int woodMinCount = 5;

    [BoxGroup("다듬기 - RindResin")]
    [InfoBox("직선 드래그 기준. 최소 횟수 미충족 시 Low 고정.")]
    [LabelText("최소 충족 횟수")]
    [SerializeField] private int rindMinCount = 5;

    [BoxGroup("다듬기 - RindResin")]
    [LabelText("허용 편차 (px)")]
    [SerializeField] private float rindAllowedDeviation = 20f;

    // 보정값 계산 후 반환
    public float CalcBonus(MatData mat, HashSet<ExtractTool.ToolType> usedTools,
        int chopCount,
        float woodAngleSum, int woodCount,
        float rindAccuracySum, int rindCount)
    {
        if (!IsCorrectToolSet(mat, usedTools))
        {
            Debug.Log($"[ExtractController] 틀린 도구 혼용 → 보정값 최저 0.5");
            return 0.5f;
        }

        float bonus = 1f;

        switch (mat.materialType)
        {
            case MaterialType.Root:
                bonus = Lerp3(chopCount, rootMin, rootOptimal, rootMax);
                Debug.Log($"[ExtractController] 칼질 {chopCount}회 → 보정값 {bonus:F2}");
                break;

            case MaterialType.Wood:
                if (woodCount < woodMinCount)
                {
                    Debug.Log($"[ExtractController] 빻기 횟수 부족 ({woodCount}/{woodMinCount}) → Low 고정");
                    return 0.5f;
                }
                bonus = Mathf.Clamp((float)woodCount / woodMinCount, 0.5f, 1.5f);
                Debug.Log($"[ExtractController] 빻기 {woodCount}회 → 보정값 {bonus:F2}");
                break;

            case MaterialType.RindResin:
                if (rindCount < rindMinCount)
                {
                    Debug.Log($"[ExtractController] 긁기 횟수 부족 ({rindCount}/{rindMinCount}) → Low 고정");
                    return 0.5f;
                }
                float avgDeviation = rindAccuracySum / rindCount;
                bonus = Mathf.Clamp(1f - (avgDeviation / rindAllowedDeviation) * 0.5f, 0.5f, 1.5f);
                Debug.Log($"[ExtractController] 긁기 평균 편차 {avgDeviation:F1}px → 보정값 {bonus:F2}");
                break;
        }

        return bonus;
    }

    // 압착 완료 시 품질 결정 및 재고 반영
    public void OnPressComplete(MatData mat, HashSet<ExtractTool.ToolType> usedTools,
        int chopCount,
        float woodAngleSum, int woodCount,
        float rindAccuracySum, int rindCount)
    {
        if (mat == null) return;

        float bonus = CalcBonus(mat, usedTools, chopCount, woodAngleSum, woodCount, rindAccuracySum, rindCount);
        Quality quality = BonusToQuality(bonus);

        if (quality == Quality.Excellent && !RuntimeState.Instance.excellentUnlocked)
        {
            quality = Quality.Good;
            Debug.Log("[ExtractController] Excellent 미해금 → Good으로 강등");
        }

        var key = (mat, quality);
        var stock = RuntimeState.Instance.stock;
        if (!stock.ContainsKey(key))
            stock[key] = 0;
        stock[key] += BaseDrop;

        Debug.Log($"[ExtractController] 압착 완료 → {mat.id} {quality} +{BaseDrop}방울 / 현재 재고 {stock[key]}방울");
    }

    // 올바른 도구만 사용했는지 확인 - 틀린 도구가 1회라도 포함되면 false
    private bool IsCorrectToolSet(MatData mat, HashSet<ExtractTool.ToolType> usedTools)
    {
        ExtractTool.ToolType correct = mat.materialType switch
        {
            MaterialType.Root => ExtractTool.ToolType.Knife,
            MaterialType.Wood => ExtractTool.ToolType.Pestle,
            MaterialType.RindResin => ExtractTool.ToolType.Scraper,
            _ => ExtractTool.ToolType.None
        };

        foreach (var tool in usedTools)
        {
            if (tool != correct)
                return false;
        }

        return true;
    }

    private Quality BonusToQuality(float bonus)
    {
        if (bonus < 0.8f) return Quality.Low;
        if (bonus < 1.1f) return Quality.Normal;
        if (bonus < 1.4f) return Quality.Good;
        return Quality.Excellent;
    }

    // 기본값 기준 구간별 선형 보간
    private float Lerp3(float value, float min, float optimal, float max)
    {
        if (value <= min) return 0.5f;
        if (value >= max) return 1.5f;
        if (value <= optimal)
            return Mathf.Lerp(0.5f, 1.0f, (value - min) / (optimal - min));
        else
            return Mathf.Lerp(1.0f, 1.5f, (value - optimal) / (max - optimal));
    }

    // 특별 공정 슬롯 (구조 확보, 내용 미구현)
    public void OnSpecialProcess()
    {
        Debug.Log("[ExtractController] 특별 공정 슬롯 (미구현)");
    }
}