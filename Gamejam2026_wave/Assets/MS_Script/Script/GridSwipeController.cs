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


        currentDragSide =
            GetDragSide(worldPosition);


        if (currentDragSide ==
            DragSide.None)
        {
            Debug.Log(
                "Grid 주변에서 시작하지 않았습니다."
            );

            return;
        }


        Debug.Log(
            "드래그 시작 위치 : " +
            currentDragSide
        );


        startWorldPosition =
            worldPosition;


        currentWidth = 1;

        isDragging = true;
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
        float distance;


        // =========================================
        // 위 / 아래에서 시작
        // → 가로 드래그
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
        // 왼쪽 / 오른쪽에서 시작
        // → 세로 드래그
        // =========================================

        else
        {
            distance =
                Mathf.Abs(
                    worldPosition.y -
                    startWorldPosition.y
                );
        }


        // =========================================
        // 최소 드래그 거리
        // =========================================

        if (distance <
            minimumDragDistance)
        {
            currentWidth = 1;

            return;
        }


        // =========================================
        // Grid 한 칸 크기
        // =========================================

        float cellSize =
            GetCellWidth();


        // =========================================
        // 몇 칸 드래그했는지
        // =========================================

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
            moveDirection
        );
    }


    // =========================================================
    // Create Block
    // =========================================================

    private void CreateBlock(
        int width,
        Vector2 moveDirection)
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


        // =========================================
        // Grid 정보
        // =========================================

        Bounds gridBounds =
            gridRenderer.bounds;


        float cellWidth =
            gridBounds.size.x /
            gridSize;


        float cellHeight =
            gridBounds.size.y /
            gridSize;


        // =========================================
        // 시작한 위치
        // =========================================

        float relativeX =
            startWorldPosition.x -
            gridBounds.min.x;


        float relativeY =
            startWorldPosition.y -
            gridBounds.min.y;


        int startColumn =
            Mathf.FloorToInt(
                relativeX /
                cellWidth
            );


        int startRow =
            Mathf.FloorToInt(
                relativeY /
                cellHeight
            );


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


        // =========================================
        // 블록 방향
        // =========================================

        bool horizontal =
            moveDirection == Vector2.up ||
            moveDirection == Vector2.down;


        // =========================================
        // 블록 크기
        // =========================================

        Vector3 blockScale;


        if (horizontal)
        {
            // 아래 / 위에서 생성
            // 가로로 긴 블록

            blockScale =
                new Vector3(
                    width,
                    1f,
                    1f
                );
        }
        else
        {
            // 왼쪽 / 오른쪽에서 생성
            // 세로로 긴 블록

            blockScale =
                new Vector3(
                    1f,
                    width,
                    1f
                );
        }


        // =========================================
        // 블록 중심 위치
        // =========================================

        float blockX =
            gridBounds.min.x +
            (startColumn * cellWidth) +
            (horizontal
                ? width * cellWidth / 2f
                : cellWidth / 2f);


        float blockY =
            gridBounds.min.y +
            (startRow * cellHeight) +
            (!horizontal
                ? width * cellHeight / 2f
                : cellHeight / 2f);


        // =========================================
        // 생성 위치
        // =========================================

        Vector3 spawnPosition;


        // =========================================
        // 아래 → 위
        // =========================================

        if (moveDirection ==
            Vector2.up)
        {
            spawnPosition =
                new Vector3(
                    blockX,
                    gridBounds.min.y -
                    cellHeight,
                    0f
                );
        }


        // =========================================
        // 위 → 아래
        // =========================================

        else if (moveDirection ==
                 Vector2.down)
        {
            spawnPosition =
                new Vector3(
                    blockX,
                    gridBounds.max.y +
                    cellHeight,
                    0f
                );
        }


        // =========================================
        // 왼쪽 → 오른쪽
        // =========================================

        else if (moveDirection ==
                 Vector2.right)
        {
            spawnPosition =
                new Vector3(
                    gridBounds.min.x -
                    cellWidth,
                    blockY,
                    0f
                );
        }


        // =========================================
        // 오른쪽 → 왼쪽
        // =========================================

        else
        {
            spawnPosition =
                new Vector3(
                    gridBounds.max.x +
                    cellWidth,
                    blockY,
                    0f
                );
        }


        // =========================================
        // 생성
        // =========================================

        GameObject block =
            Instantiate(
                blockPrefab,
                spawnPosition,
                Quaternion.identity
            );


        // =========================================
        // 크기
        // =========================================

        block.transform.localScale =
            blockScale;


        // =========================================
        // 이동 방향 설정
        // =========================================

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
            " x 1" +
            " / 방향 : " +
            moveDirection
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