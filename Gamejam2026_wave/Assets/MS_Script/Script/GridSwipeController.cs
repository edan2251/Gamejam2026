using UnityEngine;
using UnityEngine.InputSystem;

public class GridSwipeController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Transform grid;
    [SerializeField] private SpriteRenderer gridRenderer;
    [SerializeField] private int gridSize = 3;


    [Header("Block")]
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private float blockMoveSpeed = 5f;


    [Header("Drag")]
    [SerializeField] private float minimumDragDistance = 0.1f;
    [SerializeField] private int maximumBlockWidth = 3;

    [Header("Drag Zone")]
    [SerializeField] private float dragZoneThickness = 0.7f;

    private Camera mainCamera;

    private Vector2 startWorldPosition;

    private bool isDragging = false;

    private int currentWidth = 1;


    // =========================================================
    // 드래그 시작 위치
    // =========================================================

    private enum DragSide
    {
        None,
        Bottom,
        Top,
        Left,
        Right
    }

    private DragSide currentDragSide = DragSide.None;

    // =========================================================
    // Corner Drag
    // =========================================================

    // 꼭짓점 부근에서 시작했는지
    private bool startedFromCorner = false;

    // 가로 드래그일 경우 사용할 방향
    // Bottom 또는 Top
    private DragSide cornerHorizontalSide = DragSide.None;

    // 세로 드래그일 경우 사용할 방향
    // Left 또는 Right
    private DragSide cornerVerticalSide = DragSide.None;

    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        mainCamera = Camera.main;
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        HandleMouse();
    }


    // =========================================================
    // Mouse
    // =========================================================

    private void HandleMouse()
    {
        if (Mouse.current == null)
            return;


        // =========================================
        // 마우스 누름
        // =========================================

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition =
                Mouse.current.position.ReadValue();

            Vector2 worldPosition =
                ScreenToWorld(screenPosition);

            BeginDrag(worldPosition);
        }


        // =========================================
        // 드래그
        // =========================================

        if (Mouse.current.leftButton.isPressed &&
            isDragging)
        {
            Vector2 screenPosition =
                Mouse.current.position.ReadValue();

            Vector2 worldPosition =
                ScreenToWorld(screenPosition);

            UpdateDrag(worldPosition);
        }


        // =========================================
        // 마우스 뗌
        // =========================================

        if (Mouse.current.leftButton.wasReleasedThisFrame &&
            isDragging)
        {
            Vector2 screenPosition =
                Mouse.current.position.ReadValue();

            Vector2 worldPosition =
                ScreenToWorld(screenPosition);

            EndDrag(worldPosition);
        }
    }


    // =========================================================
    // Screen → World
    // =========================================================

    private Vector2 ScreenToWorld(
        Vector2 screenPosition)
    {
        Vector3 world =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(
                        mainCamera.transform.position.z
                    )
                )
            );

        return new Vector2(
            world.x,
            world.y
        );
    }


    // =========================================================
    // Drag Start
    // =========================================================

    private void BeginDrag(
    Vector2 worldPosition)
    {
        Debug.Log("드래그 시작");


        // 이전 상태 초기화
        startedFromCorner = false;

        cornerHorizontalSide =
            DragSide.None;

        cornerVerticalSide =
            DragSide.None;

        currentDragSide =
            DragSide.None;


        // =========================================================
        // 먼저 꼭짓점 영역인지 확인
        // =========================================================

        if (TryGetCornerSides(
            worldPosition,
            out DragSide horizontalSide,
            out DragSide verticalSide))
        {
            startedFromCorner = true;

            cornerHorizontalSide =
                horizontalSide;

            cornerVerticalSide =
                verticalSide;


            // 중요:
            // 지금은 방향을 결정하지 않는다.
            //
            // 실제 마우스 이동 방향을 보고
            // UpdateDrag에서 결정한다.
            currentDragSide =
                DragSide.None;


            Debug.Log(
                "꼭짓점 드래그 시작 / 가로 방향 후보 : " +
                cornerHorizontalSide +
                " / 세로 방향 후보 : " +
                cornerVerticalSide
            );
        }
        else
        {
            // =========================================
            // 일반적인 가장자리라면 기존 방식
            // =========================================

            currentDragSide =
                GetDragSide(
                    worldPosition
                );


            if (currentDragSide ==
                DragSide.None)
            {
                Debug.Log(
                    "Grid 주변에서 시작하지 않았습니다."
                );

                return;
            }


            Debug.Log(
                "일반 드래그 시작 위치 : " +
                currentDragSide
            );
        }


        // 실제 드래그 시작 위치 저장
        startWorldPosition =
            worldPosition;

        currentWidth = 1;

        isDragging = true;
    }

    // =========================================================
    // 꼭짓점 영역 검사
    // =========================================================

    private bool TryGetCornerSides(
        Vector2 worldPosition,
        out DragSide horizontalSide,
        out DragSide verticalSide)
    {
        horizontalSide =
            DragSide.None;

        verticalSide =
            DragSide.None;


        if (gridRenderer == null)
            return false;


        Bounds bounds =
            gridRenderer.bounds;


        // =========================================================
        // 각 경계까지의 거리
        // =========================================================

        float distanceBottom =
            Mathf.Abs(
                worldPosition.y -
                bounds.min.y
            );

        float distanceTop =
            Mathf.Abs(
                worldPosition.y -
                bounds.max.y
            );

        float distanceLeft =
            Mathf.Abs(
                worldPosition.x -
                bounds.min.x
            );

        float distanceRight =
            Mathf.Abs(
                worldPosition.x -
                bounds.max.x
            );


        // =========================================================
        // 범위 검사
        // =========================================================

        bool insideHorizontalRange =
            worldPosition.x >=
                bounds.min.x -
                dragZoneThickness &&
            worldPosition.x <=
                bounds.max.x +
                dragZoneThickness;


        bool insideVerticalRange =
            worldPosition.y >=
                bounds.min.y -
                dragZoneThickness &&
            worldPosition.y <=
                bounds.max.y +
                dragZoneThickness;


        bool canBottom =
            insideHorizontalRange &&
            distanceBottom <=
                dragZoneThickness;


        bool canTop =
            insideHorizontalRange &&
            distanceTop <=
                dragZoneThickness;


        bool canLeft =
            insideVerticalRange &&
            distanceLeft <=
                dragZoneThickness;


        bool canRight =
            insideVerticalRange &&
            distanceRight <=
                dragZoneThickness;


        // =========================================================
        // 가로 드래그 후보
        // Bottom / Top 중 하나
        // =========================================================

        if (canBottom || canTop)
        {
            if (canBottom && canTop)
            {
                horizontalSide =
                    distanceBottom <= distanceTop
                    ? DragSide.Bottom
                    : DragSide.Top;
            }
            else if (canBottom)
            {
                horizontalSide =
                    DragSide.Bottom;
            }
            else
            {
                horizontalSide =
                    DragSide.Top;
            }
        }


        // =========================================================
        // 세로 드래그 후보
        // Left / Right 중 하나
        // =========================================================

        if (canLeft || canRight)
        {
            if (canLeft && canRight)
            {
                verticalSide =
                    distanceLeft <= distanceRight
                    ? DragSide.Left
                    : DragSide.Right;
            }
            else if (canLeft)
            {
                verticalSide =
                    DragSide.Left;
            }
            else
            {
                verticalSide =
                    DragSide.Right;
            }
        }


        // =========================================================
        // 가로 방향 + 세로 방향을
        // 동시에 가질 때만 꼭짓점
        // =========================================================

        return
            horizontalSide != DragSide.None &&
            verticalSide != DragSide.None;
    }

    // =========================================================
    // 꼭짓점에서 실제 드래그 방향 결정
    // =========================================================

    private void ResolveCornerDirection(
        Vector2 worldPosition)
    {
        if (!startedFromCorner)
            return;


        Vector2 dragDelta =
            worldPosition -
            startWorldPosition;


        float absX =
            Mathf.Abs(
                dragDelta.x
            );


        float absY =
            Mathf.Abs(
                dragDelta.y
            );


        // =========================================
        // X 이동량이 더 큼
        // → 가로 드래그
        // → Bottom 또는 Top
        // =========================================

        if (absX > absY)
        {
            currentDragSide =
                cornerHorizontalSide;
        }


        // =========================================
        // Y 이동량이 더 큼
        // → 세로 드래그
        // → Left 또는 Right
        // =========================================

        else if (absY > absX)
        {
            currentDragSide =
                cornerVerticalSide;
        }


        // =========================================
        // 정확히 같을 경우
        // 기존 방향이 없다면 가로 우선
        // =========================================

        else if (currentDragSide ==
                 DragSide.None)
        {
            currentDragSide =
                cornerHorizontalSide;
        }
    }


    // =========================================================
    // Grid 어느 방향에서 시작했는지
    // =========================================================

    private DragSide GetDragSide(
    Vector2 worldPosition)
    {
        if (gridRenderer == null)
        {
            Debug.LogError(
                "Grid Renderer가 연결되어 있지 않습니다."
            );

            return DragSide.None;
        }

        Bounds bounds =
            gridRenderer.bounds;


        // =========================================
        // 각 경계와의 거리
        // =========================================

        float distanceBottom =
            Mathf.Abs(
                worldPosition.y -
                bounds.min.y
            );

        float distanceTop =
            Mathf.Abs(
                worldPosition.y -
                bounds.max.y
            );

        float distanceLeft =
            Mathf.Abs(
                worldPosition.x -
                bounds.min.x
            );

        float distanceRight =
            Mathf.Abs(
                worldPosition.x -
                bounds.max.x
            );


        // =========================================
        // Grid 범위 + 바깥쪽 Drag Zone
        // =========================================

        bool insideHorizontalRange =
            worldPosition.x >=
                bounds.min.x - dragZoneThickness &&
            worldPosition.x <=
                bounds.max.x + dragZoneThickness;

        bool insideVerticalRange =
            worldPosition.y >=
                bounds.min.y - dragZoneThickness &&
            worldPosition.y <=
                bounds.max.y + dragZoneThickness;


        // =========================================
        // 각 방향의 드래그 영역
        // =========================================

        bool canBottom =
            insideHorizontalRange &&
            distanceBottom <= dragZoneThickness;

        bool canTop =
            insideHorizontalRange &&
            distanceTop <= dragZoneThickness;

        bool canLeft =
            insideVerticalRange &&
            distanceLeft <= dragZoneThickness;

        bool canRight =
            insideVerticalRange &&
            distanceRight <= dragZoneThickness;


        // 어디에도 속하지 않음
        if (!canBottom &&
            !canTop &&
            !canLeft &&
            !canRight)
        {
            return DragSide.None;
        }


        // =========================================
        // 여러 영역이 겹치면
        // 가장 가까운 Grid 경계 선택
        // =========================================

        float closestDistance =
            float.MaxValue;

        DragSide closestSide =
            DragSide.None;


        if (canBottom &&
            distanceBottom < closestDistance)
        {
            closestDistance =
                distanceBottom;

            closestSide =
                DragSide.Bottom;
        }


        if (canTop &&
            distanceTop < closestDistance)
        {
            closestDistance =
                distanceTop;

            closestSide =
                DragSide.Top;
        }


        if (canLeft &&
            distanceLeft < closestDistance)
        {
            closestDistance =
                distanceLeft;

            closestSide =
                DragSide.Left;
        }


        if (canRight &&
            distanceRight < closestDistance)
        {
            closestDistance =
                distanceRight;

            closestSide =
                DragSide.Right;
        }


        return closestSide;
    }


    // =========================================================
    // Drag Update
    // =========================================================

    private void UpdateDrag(
    Vector2 worldPosition)
    {
        // =========================================================
        // 꼭짓점에서 시작했다면
        // 실제 마우스 이동 방향으로 Side 결정
        // =========================================================

        if (startedFromCorner)
        {
            ResolveCornerDirection(
                worldPosition
            );
        }


        // 아직 방향이 결정되지 않았다면
        if (currentDragSide ==
            DragSide.None)
        {
            currentWidth = 1;

            return;
        }


        float distance;

        float cellSize;


        // =========================================================
        // Bottom / Top
        // → 가로 드래그
        // =========================================================

        if (currentDragSide ==
                DragSide.Bottom ||
            currentDragSide ==
                DragSide.Top)
        {
            distance =
                Mathf.Abs(
                    worldPosition.x -
                    startWorldPosition.x
                );


            cellSize =
                gridRenderer.bounds.size.x /
                gridSize;
        }


        // =========================================================
        // Left / Right
        // → 세로 드래그
        // =========================================================

        else
        {
            distance =
                Mathf.Abs(
                    worldPosition.y -
                    startWorldPosition.y
                );


            cellSize =
                gridRenderer.bounds.size.y /
                gridSize;
        }


        // =========================================================
        // 최소 거리
        // =========================================================

        if (distance <
            minimumDragDistance)
        {
            currentWidth = 1;

            return;
        }


        // =========================================================
        // 몇 칸인지 계산
        // =========================================================

        int width =
            Mathf.RoundToInt(
                distance /
                cellSize
            );


        width =
            Mathf.Clamp(
                width,
                1,
                maximumBlockWidth
            );


        currentWidth =
            width;
    }


    // =========================================================
    // Drag End
    // =========================================================

    private void EndDrag(
    Vector2 worldPosition)
    {
        Debug.Log("드래그 종료");


        if (!isDragging)
            return;


        // =========================================================
        // 꼭짓점이었다면
        // 최종 마우스 위치를 기준으로 방향을 다시 결정
        // =========================================================

        if (startedFromCorner)
        {
            ResolveCornerDirection(
                worldPosition
            );
        }


        if (currentDragSide ==
            DragSide.None)
        {
            isDragging = false;

            return;
        }


        isDragging = false;


        float distance;


        // =========================================
        // 위 / 아래
        // =========================================

        if (currentDragSide ==
            DragSide.Bottom ||
            currentDragSide ==
            DragSide.Top)
        {
            distance =
                Mathf.Abs(
                    worldPosition.x -
                    startWorldPosition.x
                );
        }


        // =========================================
        // 왼쪽 / 오른쪽
        // =========================================

        else
        {
            distance =
                Mathf.Abs(
                    worldPosition.y -
                    startWorldPosition.y
                );
        }


        Debug.Log(
            "드래그 거리 : " +
            distance
        );


        if (distance <
            minimumDragDistance)
        {
            Debug.Log(
                "드래그 거리가 너무 짧음"
            );

            return;
        }


        int width =
            currentWidth;


        Debug.Log(
            "생성할 블록 크기 : " +
            width
        );


        // =========================================
        // 블록 이동 방향 결정
        // =========================================

        Vector2 moveDirection;


        switch (currentDragSide)
        {
            // 아래에서 시작
            // → 위로 이동
            case DragSide.Bottom:

                moveDirection =
                    Vector2.up;

                break;


            // 위에서 시작
            // → 아래로 이동
            case DragSide.Top:

                moveDirection =
                    Vector2.down;

                break;


            // 왼쪽에서 시작
            // → 오른쪽으로 이동
            case DragSide.Left:

                moveDirection =
                    Vector2.right;

                break;


            // 오른쪽에서 시작
            // → 왼쪽으로 이동
            case DragSide.Right:

                moveDirection =
                    Vector2.left;

                break;


            default:

                return;
        }


        CreateBlock(
            width,
            moveDirection,
            worldPosition
        );
    }


    // =========================================================
    // Create Block
    // =========================================================

    private void CreateBlock(
    int width,
    Vector2 moveDirection,
    Vector2 endWorldPosition)
    {
        if (blockPrefab == null)
        {
            Debug.LogError(
                "Block Prefab이 연결되지 않았습니다."
            );

            return;
        }


        if (gridRenderer == null)
        {
            Debug.LogError(
                "Grid Renderer가 연결되지 않았습니다."
            );

            return;
        }


        // =========================================================
        // Grid 정보
        // =========================================================

        Bounds gridBounds =
            gridRenderer.bounds;


        float cellWidth =
            gridBounds.size.x /
            gridSize;


        float cellHeight =
            gridBounds.size.y /
            gridSize;


        // =========================================================
        // 시작 위치가 몇 번째 칸인지 계산
        // =========================================================

        int startColumn =
            Mathf.FloorToInt(
                (
                    startWorldPosition.x -
                    gridBounds.min.x
                ) /
                cellWidth
            );


        int startRow =
            Mathf.FloorToInt(
                (
                    startWorldPosition.y -
                    gridBounds.min.y
                ) /
                cellHeight
            );


        // Grid 안으로 보정
        startColumn =
            Mathf.Clamp(
                startColumn,
                0,
                gridSize - 1
            );


        startRow =
            Mathf.Clamp(
                startRow,
                0,
                gridSize - 1
            );


        // =========================================================
        // 아래/위에서 생성되는지 확인
        // =========================================================

        bool horizontal =
            moveDirection == Vector2.up ||
            moveDirection == Vector2.down;


        // 최종적으로 사용할 시작 열/행
        int leftColumn = startColumn;
        int bottomRow = startRow;


        // =========================================================
        // 아래 / 위
        // 가로 블록
        // =========================================================

        if (horizontal)
        {
            // 실제 가로 드래그 방향
            float dragX =
                endWorldPosition.x -
                startWorldPosition.x;


            // =========================================
            // 왼쪽 → 오른쪽
            // =========================================

            if (dragX >= 0f)
            {
                // 현재 칸부터 오른쪽 끝까지
                // 몇 칸 사용 가능한지
                int availableCells =
                    gridSize -
                    startColumn;


                // Grid를 절대 넘지 않도록 제한
                width =
                    Mathf.Min(
                        width,
                        availableCells
                    );


                leftColumn =
                    startColumn;
            }


            // =========================================
            // 오른쪽 → 왼쪽
            // =========================================

            else
            {
                // 현재 칸부터 왼쪽 끝까지
                // 몇 칸 사용 가능한지
                int availableCells =
                    startColumn + 1;


                width =
                    Mathf.Min(
                        width,
                        availableCells
                    );


                // 블록의 실제 왼쪽 끝
                leftColumn =
                    startColumn -
                    width +
                    1;
            }


            // 최종 안전 보정
            width =
                Mathf.Clamp(
                    width,
                    1,
                    gridSize
                );


            leftColumn =
                Mathf.Clamp(
                    leftColumn,
                    0,
                    gridSize - width
                );
        }


        // =========================================================
        // 왼쪽 / 오른쪽
        // 세로 블록
        // =========================================================

        else
        {
            float dragY =
                endWorldPosition.y -
                startWorldPosition.y;


            // =========================================
            // 아래 → 위 드래그
            // =========================================

            if (dragY >= 0f)
            {
                int availableCells =
                    gridSize -
                    startRow;


                width =
                    Mathf.Min(
                        width,
                        availableCells
                    );


                bottomRow =
                    startRow;
            }


            // =========================================
            // 위 → 아래 드래그
            // =========================================

            else
            {
                int availableCells =
                    startRow + 1;


                width =
                    Mathf.Min(
                        width,
                        availableCells
                    );


                bottomRow =
                    startRow -
                    width +
                    1;
            }


            width =
                Mathf.Clamp(
                    width,
                    1,
                    gridSize
                );


            bottomRow =
                Mathf.Clamp(
                    bottomRow,
                    0,
                    gridSize - width
                );
        }


        // =========================================================
        // 블록 크기
        // =========================================================

        Vector3 blockScale;


        if (horizontal)
        {
            // 1x1 / 2x1 / 3x1

            blockScale =
                new Vector3(
                    width,
                    1f,
                    1f
                );
        }
        else
        {
            // 1x1 / 1x2 / 1x3

            blockScale =
                new Vector3(
                    1f,
                    width,
                    1f
                );
        }


        // =========================================================
        // 블록 중심 위치
        // =========================================================

        float blockX;

        float blockY;


        if (horizontal)
        {
            // 실제 왼쪽 칸 기준으로 중심 계산

            blockX =
                gridBounds.min.x +
                leftColumn *
                cellWidth +
                width *
                cellWidth /
                2f;


            blockY =
                gridBounds.min.y +
                cellHeight /
                2f;
        }
        else
        {
            blockX =
                gridBounds.min.x +
                cellWidth /
                2f;


            // 실제 아래쪽 Row 기준
            blockY =
                gridBounds.min.y +
                bottomRow *
                cellHeight +
                width *
                cellHeight /
                2f;
        }


        // =========================================================
        // 생성 위치
        // =========================================================

        Vector3 spawnPosition;


        // 아래에서 생성 → 위로
        if (moveDirection == Vector2.up)
        {
            spawnPosition =
                new Vector3(
                    blockX,
                    gridBounds.min.y -
                    cellHeight / 2f,
                    0f
                );
        }


        // 위에서 생성 → 아래로
        else if (moveDirection == Vector2.down)
        {
            spawnPosition =
                new Vector3(
                    blockX,
                    gridBounds.max.y +
                    cellHeight / 2f,
                    0f
                );
        }


        // 왼쪽에서 생성 → 오른쪽으로
        else if (moveDirection == Vector2.right)
        {
            spawnPosition =
                new Vector3(
                    gridBounds.min.x -
                    cellWidth / 2f,
                    blockY,
                    0f
                );
        }


        // 오른쪽에서 생성 → 왼쪽으로
        else
        {
            spawnPosition =
                new Vector3(
                    gridBounds.max.x +
                    cellWidth / 2f,
                    blockY,
                    0f
                );
        }


        // =========================================================
        // 생성
        // =========================================================

        GameObject block =
            Instantiate(
                blockPrefab,
                spawnPosition,
                Quaternion.identity
            );


        block.transform.localScale =
            blockScale;


        // =========================================================
        // 이동 방향 / 속도
        // =========================================================

        MovingBlock movingBlock =
            block.GetComponent<MovingBlock>();


        if (movingBlock != null)
        {
            movingBlock.SetMoveDirection(
                moveDirection
            );

            movingBlock.SetMoveSpeed(
                blockMoveSpeed
            );
        }


        Debug.Log(
            "블록 생성 : " +
            width +
            " / 시작 Column : " +
            startColumn +
            " / Left Column : " +
            leftColumn
        );
    }


    // =========================================================
    // Grid 아래인지 확인
    // =========================================================

    private bool IsBelowGrid(
        Vector2 worldPosition)
    {
        Bounds bounds =
            gridRenderer.bounds;


        return worldPosition.y <
               bounds.min.y;
    }


    // =========================================================
    // 한 칸 크기
    // =========================================================

    private float GetCellWidth()
    {
        Bounds bounds =
            gridRenderer.bounds;


        return bounds.size.x /
               gridSize;
    }
}