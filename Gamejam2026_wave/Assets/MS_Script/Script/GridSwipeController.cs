using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridSwipeController : MonoBehaviour
{
    [Header("========================================")]
    [Header("GRID")]
    [Header("========================================")]

    [SerializeField] private RectTransform gridRect;

    [SerializeField] private int gridSize = 3;


    [Header("========================================")]
    [Header("BLOCK")]
    [Header("========================================")]

    [SerializeField] private RectTransform squarePrefab;

    [SerializeField] private float moveDuration = 0.5f;


    [Header("========================================")]
    [Header("DRAG")]
    [Header("========================================")]

    [SerializeField] private float minimumDragDistance = 10f;

    // 최대 생성 가능한 가로 칸 수
    [SerializeField] private int maximumBlockWidth = 3;


    [Header("========================================")]
    [Header("SPAWN")]
    [Header("========================================")]

    // 그리드 아래쪽에서 시작해야 하는가?
    [SerializeField] private bool mustStartBelowGrid = true;

    // 그리드 아래쪽으로부터 얼마까지 허용할 것인지
    [SerializeField] private float spawnAreaHeight = 1000f;


    [Header("========================================")]
    [Header("GUIDE")]
    [Header("========================================")]

    // 드래그 중 나타나는 빨간색 가이드
    [SerializeField] private Image guideImage;

    [SerializeField]
    private Color guideColor =
        new Color(1f, 0f, 0f, 0.5f);


    // Canvas
    private RectTransform canvasRect;


    // 입력
    private Vector2 startScreenPosition;
    private Vector2 currentScreenPosition;

    private bool isDragging = false;
    private bool validDrag = false;


    // 현재 가이드가 차지하고 있는 칸 수
    private int currentWidth = 1;


    private void Awake()
    {
        // Canvas 찾기
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }


        // 가이드가 없으면 자동 생성
        if (guideImage == null)
        {
            CreateGuide();
        }


        HideGuide();
    }


    private void Update()
    {
        // 모바일 터치
        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        else
        {
            // PC 마우스
            HandleMouse();
        }
    }


    // =========================================================
    // MOUSE
    // =========================================================

    private void HandleMouse()
    {
        // 마우스 누름
        if (Input.GetMouseButtonDown(0))
        {
            BeginDrag(Input.mousePosition);
        }


        // 마우스 드래그 중
        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag(Input.mousePosition);
        }


        // 마우스 뗌
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag(Input.mousePosition);
        }
    }


    // =========================================================
    // TOUCH
    // =========================================================

    private void HandleTouch()
    {
        Touch touch = Input.GetTouch(0);


        // 터치 시작
        if (touch.phase == TouchPhase.Began)
        {
            BeginDrag(touch.position);
        }


        // 터치 이동
        if (
            touch.phase == TouchPhase.Moved ||
            touch.phase == TouchPhase.Stationary
        )
        {
            if (isDragging)
            {
                UpdateDrag(touch.position);
            }
        }


        // 터치 종료
        if (
            touch.phase == TouchPhase.Ended ||
            touch.phase == TouchPhase.Canceled
        )
        {
            EndDrag(touch.position);
        }
    }


    // =========================================================
    // DRAG START
    // =========================================================

    private void BeginDrag(Vector2 screenPosition)
    {
        // 이미 드래그 중이면 무시
        if (isDragging)
            return;


        // 그리드 바깥에서 시작했는지 확인
        validDrag = IsValidStartPosition(screenPosition);


        if (!validDrag)
        {
            Debug.Log("그리드 바깥의 유효한 영역에서 시작해주세요.");
            return;
        }


        startScreenPosition = screenPosition;
        currentScreenPosition = screenPosition;

        currentWidth = 1;

        isDragging = true;


        // 처음부터 1칸 가이드 표시
        UpdateGuide(screenPosition);
    }


    // =========================================================
    // DRAG UPDATE
    // =========================================================

    private void UpdateDrag(Vector2 screenPosition)
    {
        if (!isDragging)
            return;

        currentScreenPosition = screenPosition;

        Vector2 dragDelta =
            screenPosition - startScreenPosition;

        float horizontalDistance =
            Mathf.Abs(dragDelta.x);

        // 너무 조금 움직였으면 1칸
        if (horizontalDistance < minimumDragDistance)
        {
            currentWidth = 1;

            UpdateGuide(screenPosition);

            return;
        }

        // 한 칸의 실제 화면 크기
        float cellWidth =
            GetCellWidthInScreen();

        // =========================================
        // 드래그한 거리 → 칸 수
        // =========================================

        int width = Mathf.RoundToInt(
            horizontalDistance / cellWidth
        );

        // 최소 1칸
        width = Mathf.Max(1, width);

        // 최대 3칸
        width = Mathf.Min(
            maximumBlockWidth,
            width
        );

        currentWidth = width;

        // 가이드 갱신
        UpdateGuide(screenPosition);
    }


    // =========================================================
    // DRAG END
    // =========================================================

    private void EndDrag(Vector2 screenPosition)
    {
        if (!isDragging)
            return;


        isDragging = false;


        HideGuide();


        if (!validDrag)
        {
            validDrag = false;
            return;
        }


        Vector2 dragDelta =
            screenPosition - startScreenPosition;


        float distance =
            Mathf.Abs(dragDelta.x);


        // 너무 짧게 움직이면 생성하지 않음
        if (distance < minimumDragDistance)
        {
            validDrag = false;
            return;
        }


        // 현재 계산된 크기로 생성
        int width = currentWidth;


        Debug.Log(
            "블록 생성 : " +
            width +
            " x 1"
        );


        CreateBlock(width);


        validDrag = false;
    }


    // =========================================================
    // VALID START
    // =========================================================

    private bool IsValidStartPosition(
        Vector2 screenPosition)
    {
        if (gridRect == null)
            return false;


        Vector2 localPoint;


        bool success =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect,
                screenPosition,
                GetUICamera(),
                out localPoint
            );


        if (!success)
            return false;


        Rect rect = gridRect.rect;


        // 그리드 내부
        bool insideGrid =
            rect.Contains(localPoint);


        if (insideGrid)
            return false;


        // 그리드 아래쪽인지
        if (mustStartBelowGrid)
        {
            if (localPoint.y >= rect.yMin)
            {
                return false;
            }


            // 너무 멀리 떨어진 곳은 무시
            float distanceBelow =
                rect.yMin - localPoint.y;


            if (distanceBelow > spawnAreaHeight)
            {
                return false;
            }
        }


        // 여기까지 왔다면 유효
        return true;
    }


    // =========================================================
    // GUIDE CREATE
    // =========================================================

    private void CreateGuide()
    {
        if (canvasRect == null)
        {
            Debug.LogError(
                "Canvas를 찾을 수 없습니다."
            );

            return;
        }


        GameObject guideObject =
            new GameObject(
                "SwipeGuide"
            );


        guideObject.transform.SetParent(
            canvasRect,
            false
        );


        guideImage =
            guideObject.AddComponent<Image>();


        guideImage.color =
            guideColor;


        RectTransform rect =
            guideObject.GetComponent<RectTransform>();


        rect.anchorMin =
            new Vector2(0.5f, 0.5f);


        rect.anchorMax =
            new Vector2(0.5f, 0.5f);


        rect.pivot =
            new Vector2(0.5f, 0.5f);


        rect.sizeDelta =
            Vector2.zero;
    }


    // =========================================================
    // GUIDE UPDATE
    // =========================================================

    private void UpdateGuide(Vector2 screenPosition)
    {
        if (guideImage == null)
            return;

        RectTransform guideRect =
            guideImage.rectTransform;

        // =========================================
        // 드래그 방향
        // =========================================

        float direction =
            Mathf.Sign(
                screenPosition.x -
                startScreenPosition.x
            );

        // 아직 움직이지 않았다면 오른쪽
        if (direction == 0)
            direction = 1;


        // =========================================
        // 한 칸의 크기
        // =========================================

        float cellWidth =
            gridRect.rect.width / gridSize;

        float cellHeight =
            gridRect.rect.height / gridSize;


        // =========================================
        // 시작점의 Grid Local 좌표
        // =========================================

        Vector2 startLocal;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRect,
            startScreenPosition,
            GetUICamera(),
            out startLocal
        );


        // =========================================
        // 시작한 열 계산
        // =========================================

        float relativeX =
            startLocal.x -
            gridRect.rect.xMin;


        int startColumn =
            Mathf.FloorToInt(
                relativeX / cellWidth
            );


        // 범위 제한
        startColumn =
            Mathf.Clamp(
                startColumn,
                0,
                gridSize - 1
            );


        // =========================================
        // 방향에 따른 최대 크기 계산
        // =========================================

        int maxWidth;

        if (direction > 0)
        {
            // 왼쪽 → 오른쪽

            maxWidth =
                gridSize - startColumn;
        }
        else
        {
            // 오른쪽 → 왼쪽

            maxWidth =
                startColumn + 1;
        }


        // =========================================
        // 실제 사용할 폭
        // =========================================

        int width =
            Mathf.Min(
                currentWidth,
                maxWidth
            );


        width =
            Mathf.Clamp(
                width,
                1,
                maximumBlockWidth
            );


        // =========================================
        // ★ 중요 ★
        // 블록의 왼쪽 열 계산
        // =========================================

        int leftColumn;

        if (direction > 0)
        {
            // 왼쪽 → 오른쪽
            leftColumn = startColumn;
        }
        else
        {
            // 오른쪽 → 왼쪽
            leftColumn =
                startColumn - width + 1;
        }


        // 범위 보정
        leftColumn =
            Mathf.Clamp(
                leftColumn,
                0,
                gridSize - width
            );


        // =========================================
        // 가이드 크기
        // =========================================

        guideRect.sizeDelta =
            new Vector2(
                cellWidth * width,
                cellHeight
            );


        // =========================================
        // 가이드 X 위치
        // =========================================

        float targetX =
            gridRect.rect.xMin +
            (leftColumn * cellWidth) +
            (cellWidth * width / 2f);


        // =========================================
        // 가이드 Y 위치
        // =========================================

        float targetY =
            gridRect.rect.yMin -
            cellHeight / 2f;


        Vector3 worldPosition =
            gridRect.TransformPoint(
                new Vector3(
                    targetX,
                    targetY,
                    0f
                )
            );


        guideRect.position =
            worldPosition;


        guideImage.enabled = true;
    }


    // =========================================================
    // GUIDE HIDE
    // =========================================================

    private void HideGuide()
    {
        if (guideImage != null)
        {
            guideImage.enabled = false;
        }
    }


    // =========================================================
    // BLOCK CREATE
    // =========================================================

    private void CreateBlock(int width)
    {
        if (squarePrefab == null)
        {
            Debug.LogError(
                "Square Prefab이 연결되지 않았습니다!"
            );

            return;
        }


        // =========================================
        // 드래그 방향
        // =========================================

        float direction =
            Mathf.Sign(
                currentScreenPosition.x -
                startScreenPosition.x
            );

        if (direction == 0)
            direction = 1;


        // =========================================
        // Grid 한 칸 크기
        // =========================================

        float cellWidth =
            gridRect.rect.width / gridSize;

        float cellHeight =
            gridRect.rect.height / gridSize;


        // =========================================
        // 시작점의 Grid Local 좌표
        // =========================================

        Vector2 startLocal;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRect,
            startScreenPosition,
            GetUICamera(),
            out startLocal
        );


        // =========================================
        // 시작 열
        // =========================================

        float relativeX =
            startLocal.x -
            gridRect.rect.xMin;


        int startColumn =
            Mathf.FloorToInt(
                relativeX / cellWidth
            );


        startColumn =
            Mathf.Clamp(
                startColumn,
                0,
                gridSize - 1
            );


        // =========================================
        // 방향에 따른 최대 크기
        // =========================================

        int maxWidth;

        if (direction > 0)
        {
            // 왼쪽 → 오른쪽

            maxWidth =
                gridSize - startColumn;
        }
        else
        {
            // 오른쪽 → 왼쪽

            maxWidth =
                startColumn + 1;
        }


        // =========================================
        // 폭 제한
        // =========================================

        width =
            Mathf.Clamp(
                width,
                1,
                Mathf.Min(
                    maximumBlockWidth,
                    maxWidth
                )
            );


        // =========================================
        // ★ 실제 블록의 왼쪽 열 ★
        // =========================================

        int leftColumn;

        if (direction > 0)
        {
            // 왼쪽 → 오른쪽

            leftColumn =
                startColumn;
        }
        else
        {
            // 오른쪽 → 왼쪽

            leftColumn =
                startColumn - width + 1;
        }


        // 혹시 모를 범위 보정
        leftColumn =
            Mathf.Clamp(
                leftColumn,
                0,
                gridSize - width
            );


        // =========================================
        // 블록 생성
        // =========================================

        RectTransform block =
            Instantiate(
                squarePrefab,
                gridRect.parent
            );


        // =========================================
        // 정확한 크기
        // =========================================

        block.sizeDelta =
            new Vector2(
                cellWidth * width,
                cellHeight
            );


        // =========================================
        // 블록 중앙 X
        // =========================================

        float blockCenterX =
            gridRect.rect.xMin +
            (leftColumn * cellWidth) +
            (cellWidth * width / 2f);


        // =========================================
        // 그리드 아래에서 시작
        // =========================================

        float startY =
            gridRect.rect.yMin -
            cellHeight * 1.5f;


        Vector3 startWorldPosition =
            gridRect.TransformPoint(
                new Vector3(
                    blockCenterX,
                    startY,
                    0f
                )
            );


        block.position =
            startWorldPosition;

    }

    // =========================================================
    // CELL WIDTH
    // =========================================================

    private float GetCellWidthInScreen()
    {
        if (gridRect == null)
            return 1f;


        Vector3 left =
            gridRect.TransformPoint(
                new Vector3(
                    gridRect.rect.xMin,
                    0f,
                    0f
                )
            );


        Vector3 right =
            gridRect.TransformPoint(
                new Vector3(
                    gridRect.rect.xMin +
                    gridRect.rect.width /
                    gridSize,
                    0f,
                    0f
                )
            );


        Vector2 leftScreen =
            RectTransformUtility.WorldToScreenPoint(
                GetUICamera(),
                left
            );


        Vector2 rightScreen =
            RectTransformUtility.WorldToScreenPoint(
                GetUICamera(),
                right
            );


        return Vector2.Distance(
            leftScreen,
            rightScreen
        );
    }


    // =========================================================
    // UI CAMERA
    // =========================================================

    private Camera GetUICamera()
    {
        if (canvasRect == null)
            return null;


        Canvas canvas =
            canvasRect.GetComponent<Canvas>();


        if (canvas == null)
            return null;


        if (canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }


        return canvas.worldCamera;
    }
}