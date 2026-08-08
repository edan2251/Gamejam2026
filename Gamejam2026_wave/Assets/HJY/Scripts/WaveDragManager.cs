using UnityEngine;
using UnityEngine.EventSystems;

public class WaveDragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public RectTransform gridRect;
    public RectTransform dragLine;
    public WaveAnimationSpawn waveSpawner;
    public ManaManager manaManager;

    [Header("Settings")]
    [Tooltip("해당 칸을 80%(0.8) 이상 덮어야 선택됨")]
    [Range(0f, 1f)] public float snapThreshold = 0.8f;
    public float lineThickness = 30f;

    [Header("Axis Fixes (반전 스위치)")]
    [Tooltip("체크 시: X축(Top/Bottom)의 0,1,2번 칸이 거꾸로 나갈 때 켜주세요.")]
    public bool reverseXIndex = false;
    [Tooltip("체크 시: Y축(Left/Right)의 0,1,2번 칸이 거꾸로 나갈 때 켜주세요.")]
    public bool reverseYIndex = false;

    private bool isDragging = false;
    private SpawnEdge currentEdge;
    private Vector2 startLocalP;
    private float startPos;
    private float currentPos;

    public void OnBeginDrag(PointerEventData data)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, data.pressPosition, data.pressEventCamera, out startLocalP);

        if (Mathf.Abs(data.delta.x) > Mathf.Abs(data.delta.y))
        {
            currentEdge = (startLocalP.y > gridRect.rect.center.y) ? SpawnEdge.Top : SpawnEdge.Bottom;
            startPos = (startLocalP.x - gridRect.rect.xMin) / gridRect.rect.width * 3f;
        }
        else
        {
            currentEdge = (startLocalP.x > gridRect.rect.center.x) ? SpawnEdge.Right : SpawnEdge.Left;
            startPos = (gridRect.rect.yMax - startLocalP.y) / gridRect.rect.height * 3f;
        }

        currentPos = startPos;
        isDragging = true;
        dragLine.gameObject.SetActive(true);
        UpdateLineVisuals(startLocalP);
    }

    public void OnDrag(PointerEventData data)
    {
        if (!isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, data.position, data.pressEventCamera, out Vector2 currentLocalP);

        if (currentEdge == SpawnEdge.Top || currentEdge == SpawnEdge.Bottom)
            currentPos = (currentLocalP.x - gridRect.rect.xMin) / gridRect.rect.width * 3f;
        else
            currentPos = (gridRect.rect.yMax - currentLocalP.y) / gridRect.rect.height * 3f;

        currentPos = Mathf.Clamp(currentPos, -0.5f, 3.5f);
        UpdateLineVisuals(currentLocalP);
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (!isDragging) return;
        isDragging = false;
        dragLine.gameObject.SetActive(false);

        // 하얀 선이 차지하는 가장 작은 좌표와 가장 큰 좌표
        float minPos = Mathf.Min(startPos, currentPos);
        float maxPos = Mathf.Max(startPos, currentPos);

        int finalStartIdx = 3;
        int finalEndIdx = -1;

        // ★ 핵심 로직: 각 칸(0, 1, 2)마다 드래그 영역이 '해당 칸의 80%'를 덮었는지 개별 교집합 검사!
        for (int i = 0; i < 3; i++)
        {
            float cellStart = i;          // 예: 0번 칸의 시작 (0.0)
            float cellEnd = i + 1f;       // 예: 0번 칸의 끝 (1.0)

            // 드래그 선과 현재 칸이 겹치는 부분 계산
            float overlapStart = Mathf.Max(minPos, cellStart);
            float overlapEnd = Mathf.Min(maxPos, cellEnd);
            float overlap = Mathf.Max(0f, overlapEnd - overlapStart);

            // 겹친 길이가 80%(0.8) 이상일 때만 선택됨!
            if (overlap >= snapThreshold)
            {
                if (i < finalStartIdx) finalStartIdx = i;
                if (i > finalEndIdx) finalEndIdx = i;
            }
        }

        // 어떤 칸도 80%를 채우지 못했으면 취소
        if (finalEndIdx == -1)
        {
            Debug.Log("80% 이상 덮인 칸이 없어 발사가 취소되었습니다.");
            return;
        }

        // 인덱스 반전 보정 (0,1번 그었는데 1,2번 나가는 현상 해결용)
        if (currentEdge == SpawnEdge.Top || currentEdge == SpawnEdge.Bottom)
        {
            if (reverseXIndex)
            {
                int tempStart = 2 - finalEndIdx;
                int tempEnd = 2 - finalStartIdx;
                finalStartIdx = tempStart;
                finalEndIdx = tempEnd;
            }
        }
        else // Left, Right
        {
            if (reverseYIndex)
            {
                int tempStart = 2 - finalEndIdx;
                int tempEnd = 2 - finalStartIdx;
                finalStartIdx = tempStart;
                finalEndIdx = tempEnd;
            }
        }

        int waveSize = finalEndIdx - finalStartIdx + 1;

        if (manaManager != null && manaManager.currentMana < waveSize)
        {
            Debug.Log("마나 부족!");
            return;
        }

        WaveInputData inputData = new WaveInputData
        {
            edge = currentEdge,
            laneIndex = finalStartIdx,
            waveSize = waveSize
        };

        waveSpawner.SpawnWave(inputData);
    }

    private void UpdateLineVisuals(Vector2 currentLocalP)
    {
        dragLine.pivot = new Vector2(0f, 0.5f);

        if (currentEdge == SpawnEdge.Top || currentEdge == SpawnEdge.Bottom)
        {
            float yPos = (currentEdge == SpawnEdge.Top) ? gridRect.rect.yMax : gridRect.rect.yMin;
            dragLine.localPosition = new Vector2(startLocalP.x, yPos);

            float deltaX = currentLocalP.x - startLocalP.x;
            dragLine.sizeDelta = new Vector2(Mathf.Abs(deltaX), lineThickness);
            dragLine.localEulerAngles = new Vector3(0, 0, deltaX >= 0 ? 0 : 180);
        }
        else
        {
            float xPos = (currentEdge == SpawnEdge.Left) ? gridRect.rect.xMin : gridRect.rect.xMax;
            dragLine.localPosition = new Vector2(xPos, startLocalP.y);

            float deltaY = currentLocalP.y - startLocalP.y;
            dragLine.sizeDelta = new Vector2(Mathf.Abs(deltaY), lineThickness);
            dragLine.localEulerAngles = new Vector3(0, 0, deltaY >= 0 ? 90 : -90);
        }
    }
}