using UnityEngine;
using UnityEngine.EventSystems;

public class WaveDragController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("System References")]
    [Tooltip("우리가 만든 파도 생성기 연결")]
    public WaveAnimationSpawn waveSpawner;
    [Tooltip("마나 매니저 연결")]
    public ManaManager manaManager;
    [Tooltip("3x3 그리드의 UI RectTransform 연결")]
    public RectTransform gridRect;

    private Vector2 startPoint;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, eventData.position, eventData.pressEventCamera, out startPoint);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 1. 끝나는 좌표 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, eventData.position, eventData.pressEventCamera, out Vector2 endPoint);

        // 2. 좌표를 0.0 ~ 1.0 비율(정규화)로 변환 (그리드 좌하단이 0,0 / 우상단이 1,1)
        Vector2 normStart = Rect.PointToNormalized(gridRect.rect, startPoint);
        Vector2 normEnd = Rect.PointToNormalized(gridRect.rect, endPoint);

        // 그리드 바깥쪽을 너무 벗어난 터치면 무시
        if (normStart.x < -0.2f || normStart.x > 1.2f || normStart.y < -0.2f || normStart.y > 1.2f) return;

        // 3. 어느 테두리(Edge)에서 시작했는지 계산
        SpawnEdge edge = DetermineEdge(normStart);

        // 4. 인덱스(0, 1, 2) 계산
        int startIndex = 0;
        int endIndex = 0;

        if (edge == SpawnEdge.Bottom || edge == SpawnEdge.Top)
        {
            // 위/아래에서 그었으면 X축(열, Column) 인덱스 계산
            startIndex = Mathf.Clamp((int)(normStart.x * 3f), 0, 2);
            endIndex = Mathf.Clamp((int)(normEnd.x * 3f), 0, 2);

            startIndex = 2 - startIndex;
            endIndex = 2 - endIndex;
        }
        else
        {
            // 좌/우에서 그었으면 Y축(행, Row) 인덱스 계산
            int calcStart = Mathf.Clamp((int)(normStart.y * 3f), 0, 2);
            int calcEnd = Mathf.Clamp((int)(normEnd.y * 3f), 0, 2);

            startIndex = 2 - calcStart;
            endIndex = 2 - calcEnd;
        }

        // 5. 파도 크기 및 마나 계산
        int waveSize = Mathf.Abs(endIndex - startIndex) + 1;

        if (manaManager != null)
        {
            if (manaManager.currentMana < waveSize)
            {
                Debug.Log($"마나 부족! (필요: {waveSize}, 현재: {manaManager.currentMana})");
                return;
            }
            // 마나 소모 실행
            manaManager.UseMana(waveSize);
        }

        // 6. 계산된 데이터를 포장해서 우리가 만든 스폰 매니저에 쏘기!
        WaveInputData inputData = new WaveInputData
        {
            edge = edge,
            //startIndex = Mathf.Min(startIndex, endIndex), // 항상 작은 숫자가 앞에 오도록 정렬
            //endIndex = Mathf.Max(startIndex, endIndex)
        };

        waveSpawner.SpawnWave(inputData);
    }

    // 터치 시작점이 4면의 테두리 중 어디에 가장 가까운지 찾는 수학 함수
    private SpawnEdge DetermineEdge(Vector2 normStart)
    {
        float distBottom = normStart.y;             // 바닥과의 거리
        float distTop = 1f - normStart.y;           // 천장과의 거리
        float distLeft = normStart.x;               // 왼쪽과의 거리
        float distRight = 1f - normStart.x;         // 오른쪽과의 거리

        float min = Mathf.Min(distBottom, distTop, distLeft, distRight);

        if (min == distBottom) return SpawnEdge.Bottom;
        if (min == distTop) return SpawnEdge.Top;
        if (min == distLeft) return SpawnEdge.Left;
        return SpawnEdge.Right;
    }
}