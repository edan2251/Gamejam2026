using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridSwipeController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private RectTransform gridRect;
    [SerializeField] private int gridSize = 3;

    [Header("Square")]
    [SerializeField] private RectTransform squarePrefab;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.5f;

    [Header("Swipe")]
    [SerializeField] private float minimumSwipeDistance = 20f;

    // 입력 시작 위치
    private Vector2 startInputPosition;

    // 입력 중인지
    private bool isInputting;

    // 그리드 아래에서 시작했는지
    private bool validSwipe;


    private void Update()
    {
        // 모바일
        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        // PC 마우스
        else
        {
            HandleMouse();
        }
    }


    // =========================================================
    // 모바일 터치
    // =========================================================

    private void HandleTouch()
    {
        Touch touch = Input.GetTouch(0);

        // 손가락을 화면에 처음 댐
        if (touch.phase == TouchPhase.Began)
        {
            startInputPosition = touch.position;

            isInputting = true;

            validSwipe = IsBelowGrid(startInputPosition);
        }

        // 손가락을 뗌
        if (touch.phase == TouchPhase.Ended)
        {
            if (isInputting && validSwipe)
            {
                ProcessSwipe(
                    startInputPosition,
                    touch.position
                );
            }

            isInputting = false;
            validSwipe = false;
        }
    }


    // =========================================================
    // PC 마우스
    // =========================================================

    private void HandleMouse()
    {
        // 마우스 버튼 누름
        if (Input.GetMouseButtonDown(0))
        {
            startInputPosition = Input.mousePosition;

            isInputting = true;

            validSwipe = IsBelowGrid(startInputPosition);
        }

        // 마우스 버튼 뗌
        if (Input.GetMouseButtonUp(0))
        {
            if (isInputting && validSwipe)
            {
                ProcessSwipe(
                    startInputPosition,
                    Input.mousePosition
                );
            }

            isInputting = false;
            validSwipe = false;
        }
    }


    // =========================================================
    // 그리드 아래쪽에서 시작했는지 확인
    // =========================================================

    private bool IsBelowGrid(Vector2 screenPosition)
    {
        Vector3[] corners = new Vector3[4];

        gridRect.GetWorldCorners(corners);

        // 왼쪽 아래
        Vector3 bottomLeft = corners[0];

        // 오른쪽 아래
        Vector3 bottomRight = corners[3];

        float bottom = bottomLeft.y;

        float left = bottomLeft.x;
        float right = bottomRight.x;

        // 그리드 바로 아래 + 그리드의 가로 범위에서 시작해야 함
        bool isBelow = screenPosition.y < bottom;

        bool isInsideHorizontal =
            screenPosition.x >= left &&
            screenPosition.x <= right;

        return isBelow && isInsideHorizontal;
    }


    // =========================================================
    // 스와이프 계산
    // =========================================================

    private void ProcessSwipe(
        Vector2 start,
        Vector2 end)
    {
        Vector2 difference = end - start;

        // 너무 짧은 움직임 무시
        if (difference.magnitude < minimumSwipeDistance)
        {
            Debug.Log("스와이프 거리가 너무 짧습니다.");
            return;
        }

        // 가로 스와이프만 허용
        if (Mathf.Abs(difference.x) < Mathf.Abs(difference.y))
        {
            Debug.Log("세로 스와이프는 무시합니다.");
            return;
        }

        // 한 칸의 크기
        float cellWidth =
            gridRect.rect.width / gridSize;

        // 몇 칸을 이동했는지 계산
        int draggedCells = Mathf.RoundToInt(
            Mathf.Abs(difference.x) / cellWidth
        );

        // 최소 1칸
        draggedCells = Mathf.Max(1, draggedCells);

        // 최대 그리드 크기
        draggedCells = Mathf.Min(
            gridSize,
            draggedCells
        );

        Debug.Log(
            "스와이프한 칸 수 : " + draggedCells
        );

        CreateSquare(draggedCells);
    }


    // =========================================================
    // 네모 생성
    // =========================================================

    private void CreateSquare(int size)
    {
        if (squarePrefab == null)
        {
            Debug.LogError(
                "Square Prefab이 연결되지 않았습니다!"
            );

            return;
        }

        RectTransform square =
            Instantiate(
                squarePrefab,
                gridRect.parent
            );

        // 한 칸의 크기
        float cellWidth =
            gridRect.rect.width / gridSize;

        float cellHeight =
            gridRect.rect.height / gridSize;


        // =====================================================
        // 네모 크기
        // =====================================================

        square.sizeDelta = new Vector2(
            cellWidth * size,
            cellHeight * size
        );


        // =====================================================
        // 그리드 위치 계산
        // =====================================================

        Vector3[] corners = new Vector3[4];

        gridRect.GetWorldCorners(corners);

        Vector3 bottomLeft = corners[0];

        Vector3 bottomRight = corners[3];

        float centerX =
            (bottomLeft.x + bottomRight.x) / 2f;


        // =====================================================
        // 시작 위치
        // 그리드 바로 아래
        // =====================================================

        float startY =
            bottomLeft.y -
            (cellHeight * size) / 2f;


        Vector3 startPosition =
            new Vector3(
                centerX,
                startY,
                square.position.z
            );

        square.position = startPosition;


        // =====================================================
        // 목표 위치
        // =====================================================

        float targetY =
            bottomLeft.y +
            (cellHeight * size) / 2f;


        Vector3 targetPosition =
            new Vector3(
                centerX,
                targetY,
                square.position.z
            );


        // =====================================================
        // 위쪽으로 이동
        // =====================================================

        StartCoroutine(
            MoveSquare(
                square,
                targetPosition
            )
        );
    }


    // =========================================================
    // 네모 이동
    // =========================================================

    private IEnumerator MoveSquare(
        RectTransform square,
        Vector3 targetPosition)
    {
        Vector3 startPosition =
            square.position;

        float time = 0f;


        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t =
                time / moveDuration;

            // 부드러운 이동
            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            square.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }


        square.position = targetPosition;
    }
}