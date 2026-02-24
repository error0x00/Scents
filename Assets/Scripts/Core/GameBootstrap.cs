using UnityEngine;

// Atelier 씬 시작 시 테스트 재고를 세팅
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private MatData[] testMaterials;
    [SerializeField] private int[] testAmounts;

    [SerializeField] private ExtractTool extractTool;

    private void Start()
    {
        SetupTestStock();
        LogStock();
        extractTool?.RefreshShelf();
    }

    // 테스트용 재고 데이터 세팅 (품질 Normal 고정)
    private void SetupTestStock()
    {
        if (testMaterials == null || testAmounts == null)
        {
            Debug.LogWarning("[GameBootstrap] 테스트 재료가 연결되지 않았습니다.");
            return;
        }

        var stock = RuntimeState.Instance.stock;

        for (int i = 0; i < testMaterials.Length; i++)
        {
            if (testMaterials[i] == null) continue;
            int amount = i < testAmounts.Length ? testAmounts[i] : 0;
            stock[(testMaterials[i], Quality.Normal)] = amount;
        }
    }

    // Quantum Console용 재고 로그 출력
    private void LogStock()
    {
        foreach (var pair in RuntimeState.Instance.stock)
            Debug.Log($"[GameBootstrap] 재고 - {pair.Key.mat.id} ({pair.Key.quality}) : {pair.Value}방울");
    }
}