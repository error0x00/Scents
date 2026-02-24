using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;

// 추출 공정의 모든 조작 입력을 담당
public class ExtractTool : MonoBehaviour
{
    public enum ToolType { None, Knife, Scraper, Pestle }

    // 재료별 공정 상태
    private class TrimState
    {
        public HashSet<ToolType> usedTools = new HashSet<ToolType>();
        public int chopCount = 0;
        public float woodAngleSum = 0f;
        public int woodCount = 0;
        public float woodCurrentAngle = 0f;
        public float rindAccuracySum = 0f;
        public int rindCount = 0;
    }

    [BoxGroup("영역")]
    [InfoBox("선반, 도마, 압착기 영역 오브젝트를 연결하세요.")]
    [LabelText("선반 영역")]
    [SerializeField] private RectTransform shelfArea;

    [BoxGroup("영역")]
    [LabelText("도마 영역")]
    [SerializeField] private RectTransform boardArea;

    [BoxGroup("영역")]
    [LabelText("압착기 영역")]
    [SerializeField] private RectTransform pressArea;

    [BoxGroup("도구 오브젝트")]
    [InfoBox("씬에 배치된 도구 오브젝트를 연결하세요.")]
    [LabelText("칼")]
    [SerializeField] private RectTransform knifeObj;

    [BoxGroup("도구 오브젝트")]
    [LabelText("긁개")]
    [SerializeField] private RectTransform scraperObj;

    [BoxGroup("도구 오브젝트")]
    [LabelText("막자")]
    [SerializeField] private RectTransform pestleObj;

    [BoxGroup("선반")]
    [InfoBox("선반에 재료 아이템을 생성할 프리팹을 연결하세요.")]
    [LabelText("재료 아이템 부모 오브젝트")]
    [SerializeField] private RectTransform shelfContent;

    [BoxGroup("선반")]
    [LabelText("재료 아이템 프리팹")]
    [SerializeField] private GameObject matItemPrefab;

    [BoxGroup("압착")]
    [LabelText("압착 완료까지 누르는 시간 (초)")]
    [SerializeField] private float pressHoldTime = 2f;

    private ExtractController controller;

    [ShowInInspector, ReadOnly]
    private ToolType currentTool = ToolType.None;

    [ShowInInspector, ReadOnly]
    private MatData boardMat = null;

    private bool toolFollowing = false;
    private RectTransform followingToolObj = null;
    private CanvasGroup followingToolCanvas = null;

    private Vector2 knifeOrigin;
    private Vector2 scraperOrigin;
    private Vector2 pestleOrigin;

    private bool matDragging = false;
    private MatData draggingMat = null;
    private RectTransform draggingMatObj = null;
    private CanvasGroup draggingMatCanvas = null;
    private Vector2 matDragStartPos;

    private RectTransform boardMatObj = null;
    private CanvasGroup boardMatCanvas = null;
    private bool boardMatDragging = false;

    // 재료별 공정 상태 저장
    private Dictionary<MatData, TrimState> trimStates = new Dictionary<MatData, TrimState>();

    private bool woodDragging = false;
    private float woodPrevAngle = 0f;
    private bool rindDragging = false;
    private Vector2 rindDragStart = Vector2.zero;
    private Vector2 rindDragEnd = Vector2.zero;
    private List<Vector2> rindPath = new List<Vector2>();

    private bool isPressing = false;
    private float pressElapsed = 0f;

    private bool trimActive = false;

    private void Awake()
    {
        controller = GetComponent<ExtractController>();

        if (knifeObj != null) knifeOrigin = knifeObj.anchoredPosition;
        if (scraperObj != null) scraperOrigin = scraperObj.anchoredPosition;
        if (pestleObj != null) pestleOrigin = pestleObj.anchoredPosition;
    }

    private void Update()
    {
        if (toolFollowing)
        {
            UpdateToolFollow();
            CheckDropTool();
        }

        if (isPressing)
            UpdatePress();
    }

    // 도구를 들고 있을 때 도마/압착기 밖 클릭 시 그 자리에 내려놓음
    private void CheckDropTool()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (!IsInArea(mousePos, boardArea) && !IsInArea(mousePos, pressArea))
                DropToolAtPosition(mousePos);
        }
    }

    // 도구를 현재 마우스 위치에 내려놓음
    private void DropToolAtPosition(Vector2 screenPos)
    {
        if (followingToolObj == null) return;

        if (followingToolCanvas != null)
            followingToolCanvas.blocksRaycasts = true;

        followingToolObj.position = screenPos;

        toolFollowing = false;
        followingToolObj = null;
        followingToolCanvas = null;
        currentTool = ToolType.None;
        Debug.Log("[ExtractTool] 도구 내려놓음");
    }

    // 현재 재료의 공정 상태 가져오기 (없으면 새로 생성)
    private TrimState GetTrimState(MatData mat)
    {
        if (!trimStates.ContainsKey(mat))
            trimStates[mat] = new TrimState();
        return trimStates[mat];
    }

    // 선반 재료 목록 갱신 (GameBootstrap에서 재고 세팅 후 호출)
    public void RefreshShelf()
    {
        if (shelfContent == null)
        {
            Debug.LogWarning("[ExtractTool] shelfContent가 연결되지 않았습니다.");
            return;
        }

        if (matItemPrefab == null)
        {
            Debug.LogWarning("[ExtractTool] matItemPrefab이 연결되지 않았습니다.");
            return;
        }

        foreach (Transform child in shelfContent)
            Destroy(child.gameObject);

        var seen = new HashSet<MatData>();
        foreach (var pair in RuntimeState.Instance.stock)
        {
            if (pair.Value <= 0) continue;
            if (seen.Contains(pair.Key.mat)) continue;
            seen.Add(pair.Key.mat);

            var item = Instantiate(matItemPrefab, shelfContent);
            var trigger = item.GetComponent<EventTrigger>();
            if (trigger == null) trigger = item.AddComponent<EventTrigger>();

            // 재료 이름 표시 (다국어 시스템 완성 전까지 displayName 사용)
            var label = item.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = pair.Key.mat.displayName;

            MatData mat = pair.Key.mat;

            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            entryDown.callback.AddListener((data) => OnMatDragBegin(mat, item.GetComponent<RectTransform>(), item.GetComponent<CanvasGroup>(), (PointerEventData)data));
            trigger.triggers.Add(entryDown);

            var entryDrag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            entryDrag.callback.AddListener((data) => OnMatDrag((PointerEventData)data));
            trigger.triggers.Add(entryDrag);

            var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            entryUp.callback.AddListener((data) => OnMatDragEnd((PointerEventData)data));
            trigger.triggers.Add(entryUp);

            Debug.Log($"[ExtractTool] 선반 표시: {mat.displayName}");
        }
    }

    private void OnMatDragBegin(MatData mat, RectTransform obj, CanvasGroup cg, PointerEventData data)
    {
        if (boardMat != null) return;
        draggingMat = mat;
        draggingMatObj = obj;
        draggingMatCanvas = cg;
        matDragStartPos = obj.anchoredPosition;
        matDragging = true;
        Debug.Log($"[ExtractTool] 재료 드래그 시작: {mat.displayName}");
    }

    private void OnMatDrag(PointerEventData data)
    {
        if (!matDragging || draggingMatObj == null) return;
        draggingMatObj.position = data.position;
    }

    private void OnMatDragEnd(PointerEventData data)
    {
        if (!matDragging) return;
        matDragging = false;

        if (IsInArea(data.position, boardArea))
        {
            boardMat = draggingMat;
            boardMatObj = draggingMatObj;
            boardMatCanvas = draggingMatCanvas;

            if (boardMatCanvas != null)
                boardMatCanvas.blocksRaycasts = false;

            trimActive = true;
            Debug.Log($"[ExtractTool] 도마에 재료 올림: {boardMat.displayName}");
        }
        else if (IsInArea(data.position, pressArea))
        {
            boardMat = draggingMat;
            boardMatObj = draggingMatObj;
            boardMatCanvas = draggingMatCanvas;

            if (boardMatCanvas != null)
                boardMatCanvas.blocksRaycasts = false;

            trimActive = false;
            Debug.Log($"[ExtractTool] 압착기에 재료 올림: {boardMat.displayName}");
        }
        else
        {
            Debug.Log("[ExtractTool] 재료 내려놓음");
        }

        draggingMat = null;
        draggingMatObj = null;
        draggingMatCanvas = null;
    }

    public void OnKnifeClick()
    {
        PickTool(ToolType.Knife, knifeObj);
    }

    public void OnScraperClick()
    {
        PickTool(ToolType.Scraper, scraperObj);
    }

    public void OnPestleClick()
    {
        PickTool(ToolType.Pestle, pestleObj);
    }

    private void PickTool(ToolType type, RectTransform obj)
    {
        if (toolFollowing && followingToolObj != null)
            PutDownTool();

        currentTool = type;
        followingToolObj = obj;
        followingToolCanvas = obj.GetComponent<CanvasGroup>();

        if (followingToolCanvas != null)
            followingToolCanvas.blocksRaycasts = false;

        toolFollowing = true;
        Debug.Log($"[ExtractTool] 도구 선택: {type}");
    }

    private void UpdateToolFollow()
    {
        if (followingToolObj == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        followingToolObj.position = mousePos;
    }

    private void PutDownTool()
    {
        if (followingToolObj == null) return;

        if (followingToolCanvas != null)
            followingToolCanvas.blocksRaycasts = true;

        switch (currentTool)
        {
            case ToolType.Knife: followingToolObj.anchoredPosition = knifeOrigin; break;
            case ToolType.Scraper: followingToolObj.anchoredPosition = scraperOrigin; break;
            case ToolType.Pestle: followingToolObj.anchoredPosition = pestleOrigin; break;
        }

        toolFollowing = false;
        followingToolObj = null;
        followingToolCanvas = null;
        currentTool = ToolType.None;
        Debug.Log("[ExtractTool] 도구 내려놓음");
    }

    // 도마 클릭 - 칼을 들고 있을 때만 칼질로 인정
    public void OnBoardClick(BaseEventData data)
    {
        if (!trimActive || boardMat == null) return;
        if (!toolFollowing) return;
        if (currentTool != ToolType.Knife) return;

        var state = GetTrimState(boardMat);
        state.usedTools.Add(ToolType.Knife);
        state.chopCount++;
        Debug.Log($"[ExtractTool] 칼질 {state.chopCount}회");
    }

    // 도마 드래그 시작 - 도구를 들고 있으면 공정 시작, 아니면 재료 이동
    public void OnBoardDragBegin(BaseEventData data)
    {
        if (matDragging) return;
        if (boardMat == null) return;

        if (toolFollowing)
        {
            var pd = (PointerEventData)data;

            if (currentTool == ToolType.Pestle && trimActive)
            {
                woodPrevAngle = Mathf.Atan2(pd.position.y - boardArea.position.y, pd.position.x - boardArea.position.x) * Mathf.Rad2Deg;
                woodDragging = true;
                var state = GetTrimState(boardMat);
                state.usedTools.Add(ToolType.Pestle);
                Debug.Log("[ExtractTool] 빻기 드래그 시작");
            }
            else if (currentTool == ToolType.Scraper && trimActive)
            {
                rindDragStart = pd.position;
                rindPath.Clear();
                rindPath.Add(pd.position);
                rindDragging = true;
                var state = GetTrimState(boardMat);
                state.usedTools.Add(ToolType.Scraper);
                Debug.Log("[ExtractTool] 긁기 드래그 시작");
            }
            return;
        }

        boardMatDragging = true;
        Debug.Log("[ExtractTool] 도마 위 재료 드래그 시작");
    }

    // 도마 드래그 - 도구 공정 측정 또는 재료 이동
    public void OnBoardDrag(BaseEventData data)
    {
        if (matDragging) return;
        if (boardMat == null) return;
        var pd = (PointerEventData)data;

        if (boardMatDragging)
        {
            if (boardMatObj != null)
                boardMatObj.position = pd.position;
            return;
        }

        if (!trimActive) return;

        if (woodDragging && currentTool == ToolType.Pestle)
        {
            var state = GetTrimState(boardMat);
            float angle = Mathf.Atan2(pd.position.y - boardArea.position.y, pd.position.x - boardArea.position.x) * Mathf.Rad2Deg;
            float delta = Mathf.Abs(Mathf.DeltaAngle(woodPrevAngle, angle));
            state.woodCurrentAngle += delta;
            woodPrevAngle = angle;

            // 360도마다 1회 카운트
            while (state.woodCurrentAngle >= 360f)
            {
                state.woodCurrentAngle -= 360f;
                state.woodAngleSum += 360f;
                state.woodCount++;
                Debug.Log($"[ExtractTool] 빻기 {state.woodCount}회 완료");
            }
        }
        else if (rindDragging && currentTool == ToolType.Scraper)
        {
            rindPath.Add(pd.position);
            rindDragEnd = pd.position;
        }
    }

    // 도마 드래그 끝
    public void OnBoardDragEnd(BaseEventData data)
    {
        if (matDragging) return;
        if (boardMat == null) return;
        var pd = (PointerEventData)data;

        if (boardMatDragging)
        {
            boardMatDragging = false;

            if (IsInArea(pd.position, pressArea))
            {
                if (boardMatCanvas != null)
                    boardMatCanvas.blocksRaycasts = false;

                var state = GetTrimState(boardMat);
                float bonus = controller.CalcBonus(boardMat, state.usedTools, state.chopCount, state.woodAngleSum, state.woodCount, state.rindAccuracySum, state.rindCount);
                Debug.Log($"[ExtractTool] 압착기로 이동 → 보정값 {bonus:F2}");
                trimActive = false;
            }
            else
            {
                if (boardMatCanvas != null)
                    boardMatCanvas.blocksRaycasts = true;

                Debug.Log("[ExtractTool] 재료 도마 밖으로 이동");
                boardMat = null;
                boardMatObj = null;
                boardMatCanvas = null;
                trimActive = false;
            }
            return;
        }

        if (woodDragging && currentTool == ToolType.Pestle)
            woodDragging = false;

        if (rindDragging && currentTool == ToolType.Scraper)
        {
            rindDragging = false;
            var state = GetTrimState(boardMat);
            float deviation = CalcLineDeviation(rindDragStart, rindDragEnd, rindPath);
            state.rindAccuracySum += deviation;
            state.rindCount++;
            rindPath.Clear();
            Debug.Log($"[ExtractTool] 긁기 {state.rindCount}회 완료 → 편차 {deviation:F1}px");
        }
    }

    // 드래그 경로의 직선 편차 계산
    private float CalcLineDeviation(Vector2 start, Vector2 end, List<Vector2> path)
    {
        if (path.Count < 2) return 0f;

        Vector2 line = end - start;
        float lineLen = line.magnitude;
        if (lineLen < 0.001f) return 0f;

        float totalDev = 0f;
        foreach (var point in path)
        {
            float dev = Mathf.Abs((point.x - start.x) * (end.y - start.y) - (point.y - start.y) * (end.x - start.x)) / lineLen;
            totalDev += dev;
        }
        return totalDev / path.Count;
    }

    public void OnPressDown(BaseEventData data)
    {
        if (boardMat == null) return;
        isPressing = true;
        pressElapsed = 0f;
        Debug.Log("[ExtractTool] 압착 시작");
    }

    public void OnPressUp(BaseEventData data)
    {
        if (!isPressing) return;
        isPressing = false;
        if (pressElapsed < pressHoldTime)
            Debug.Log($"[ExtractTool] 압착 미완료 ({pressElapsed:F1}초 / 필요 {pressHoldTime}초)");
    }

    private void UpdatePress()
    {
        pressElapsed += Time.deltaTime;
        if (pressElapsed >= pressHoldTime)
        {
            isPressing = false;

            var state = GetTrimState(boardMat);
            controller.OnPressComplete(boardMat, state.usedTools, state.chopCount, state.woodAngleSum, state.woodCount, state.rindAccuracySum, state.rindCount);

            // 압착 완료 후 재료 공정 상태 제거
            trimStates.Remove(boardMat);

            if (boardMatCanvas != null)
                boardMatCanvas.blocksRaycasts = true;

            boardMat = null;
            boardMatObj = null;
            boardMatCanvas = null;
            trimActive = false;
            RefreshShelf();
            Debug.Log("[ExtractTool] 압착 완료");
        }
    }

    private bool IsInArea(Vector2 screenPos, RectTransform area)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(area, screenPos);
    }

    // 특별 공정 슬롯 (구조 확보, 내용 미구현)
    public void OnSpecialProcess()
    {
        Debug.Log("[ExtractTool] 특별 공정 슬롯 (미구현)");
    }
}