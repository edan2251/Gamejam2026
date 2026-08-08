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


    private Camera mainCamera;

    private Vector2 startWorldPosition;

    private bool isDragging = false;

    private int currentWidth = 1;


    private void Awake()
    {
        mainCamera = Camera.main;
    }


    private void Update()
    {
        HandleMouse();
    }


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

    private Vector2 ScreenToWorld(Vector2 screenPosition)
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

    //private void BeginDrag(Vector2 worldPosition)
    //{
    //    // 반드시 Grid 아래쪽에서 시작
    //    if (!IsBelowGrid(worldPosition))
    //    {
    //        return;
    //    }


    //    startWorldPosition =
    //        worldPosition;

    //    currentWidth = 1;

    //    isDragging = true;
    //}

    private void BeginDrag(Vector2 worldPosition)
    {
        Debug.Log("드래그 시작");

        if (!IsBelowGrid(worldPosition))
        {
            Debug.Log("Grid 아래가 아님");
            return;
        }

        Debug.Log("Grid 아래에서 드래그 시작");

        startWorldPosition = worldPosition;

        currentWidth = 1;

        isDragging = true;
    }

    // =========================================================
    // Drag Update
    // =========================================================

    private void UpdateDrag(Vector2 worldPosition)
    {
        float distance =
            Mathf.Abs(
                worldPosition.x -
                startWorldPosition.x
            );


        if (distance <
            minimumDragDistance)
        {
            currentWidth = 1;

            return;
        }


        // Grid 한 칸의 실제 월드 크기
        float cellWidth =
            GetCellWidth();


        int width =
            Mathf.RoundToInt(
                distance / cellWidth
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

    //private void EndDrag(Vector2 worldPosition)
    //{
    //    if (!isDragging)
    //        return;

    //    isDragging = false;


    //    float distance =
    //        Mathf.Abs(
    //            worldPosition.x -
    //            startWorldPosition.x
    //        );


    //    if (distance <
    //        minimumDragDistance)
    //    {
    //        return;
    //    }


    //    int width =
    //        currentWidth;


    //    float direction =
    //        Mathf.Sign(
    //            worldPosition.x -
    //            startWorldPosition.x
    //        );


    //    if (direction == 0)
    //        direction = 1;


    //    CreateBlock(width, direction);
    //}

    private void EndDrag(Vector2 worldPosition)
    {
        Debug.Log("드래그 종료");

        if (!isDragging)
            return;

        isDragging = false;

        float distance =
            Mathf.Abs(
                worldPosition.x -
                startWorldPosition.x
            );

        Debug.Log(
            "드래그 거리 : " +
            distance
        );

        if (distance < minimumDragDistance)
        {
            Debug.Log("드래그 거리가 너무 짧음");
            return;
        }

        int width =
            currentWidth;

        Debug.Log(
            "생성할 블록 크기 : " +
            width
        );

        // 드래그 방향 계산
        float direction =
            Mathf.Sign(
                worldPosition.x -
                startWorldPosition.x
            );

        if (direction == 0)
            direction = 1;

        CreateBlock(width, direction);
    }



    // =========================================================
    // Create Block
    // =========================================================

    private void CreateBlock(
    int width,
    float direction)
    {
        if (blockPrefab == null)
        {
            Debug.LogError(
                "Block Prefab이 연결되지 않았습니다."
            );

            return;
        }


        // =========================================
        // 드래그 방향
        // =========================================


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
        // 시작한 열
        // =========================================

        float relativeX =
            startWorldPosition.x -
            gridBounds.min.x;


        int startColumn =
            Mathf.FloorToInt(
                relativeX /
                cellWidth
            );


        startColumn =
            Mathf.Clamp(
                startColumn,
                0,
                gridSize - 1
            );


        // =========================================
        // 방향에 따른 왼쪽 열
        // =========================================

        int leftColumn;


        if (direction > 0)
        {
            leftColumn =
                startColumn;
        }
        else
        {
            leftColumn =
                startColumn -
                width +
                1;
        }


        // =========================================
        // 범위 보정
        // =========================================

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


        // =========================================
        // Block 크기
        // =========================================

        Vector3 blockScale =
            new Vector3(
                width,
                1f,
                1f
            );


        // =========================================
        // Block 중심
        // =========================================

        float blockX =
            gridBounds.min.x +
            (leftColumn * cellWidth) +
            (width * cellWidth / 2f);


        // =========================================
        // Grid 아래쪽에서 생성
        // =========================================

        float blockY =
            gridBounds.min.y -
            cellHeight;


        Vector3 spawnPosition =
            new Vector3(
                blockX,
                blockY,
                0f
            );


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
            new Vector3(
                width,
                1f,
                1f
            );


        // 이동 속도 설정
        MovingBlock movingBlock =
            block.GetComponent<MovingBlock>();


        if (movingBlock != null)
        {
            // Inspector에서 기본값을 사용
        }


        Debug.Log(
            "블록 생성 : " +
            width +
            " x 1"
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