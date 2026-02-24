using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// 게임 전체에서 공유하는 런타임 상태
public class RuntimeState : MonoBehaviour
{
    public static RuntimeState Instance { get; private set; }

    // 향료 에셋과 품질별 보유 방울 수 (MatData + Quality → 방울 수)
    [ShowInInspector]
    public Dictionary<(MatData mat, Quality quality), int> stock
        = new Dictionary<(MatData, Quality), int>();

    // Excellent 품질 해금 여부 (스킬 해금 시 true로 변경)
    [ShowInInspector]
    public bool excellentUnlocked = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}