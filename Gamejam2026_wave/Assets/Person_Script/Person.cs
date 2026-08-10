using UnityEngine;
using System.Collections.Generic;

public class Person : MonoBehaviour
{
    public enum PersonState { Moving, Swept }
    public PersonState currentState = PersonState.Moving;

    [Header("UI References")]
    public RectTransform rectTransform;

    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 200f;
    [SerializeField] private float acceleration = 150f;

    [Header("Slow Down")]
    [SerializeField] private float slowDownDistance = 100f;
    [SerializeField] private float destroyDistance = 5f;

    [Header("Path")]
    [SerializeField] private PathDot pathDotPrefab;
    [SerializeField] private float dotSpacing = 30f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float currentMoveSpeed = 0f;
    private RectTransform target;
    private RectTransform uiParent;

    // 이 사람이 생성될 때 만들어진 PathDot들을 추적하기 위한 리스트
    private List<PathDot> myDots = new List<PathDot>();

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
    }

    public void SetTarget(RectTransform target, RectTransform parentCanvas)
    {
        this.target = target;
        this.uiParent = parentCanvas;
        CreatePath();
    }

    private void CreatePath()
    {
        if (target == null || pathDotPrefab == null) return;

        float distance = Vector2.Distance(rectTransform.anchoredPosition, target.anchoredPosition);
        int dotCount = Mathf.FloorToInt(distance / dotSpacing);

        for (int i = 1; i <= dotCount; i++)
        {
            float ratio = (float)i / dotCount;
            Vector2 position = Vector2.Lerp(rectTransform.anchoredPosition, target.anchoredPosition, ratio);

            PathDot dot = Instantiate(pathDotPrefab, uiParent);
            dot.GetComponent<RectTransform>().anchoredPosition = position;
            dot.SetPerson(rectTransform);

            // 내가 만든 닷을 리스트에 등록해 둠
            myDots.Add(dot);
        }
    }

    private void Update()
    {
        if (target == null) return;

        // 파도에 쓸려가는 중이면 WaveFront가 위치를 강제로 이동시키므로 일반 이동 로직은 쉽니다.
        if (currentState == PersonState.Swept)
        {
            return;
        }

        // --- 일반 이동 로직 ---
        float distance = Vector2.Distance(rectTransform.anchoredPosition, target.anchoredPosition);

        if (distance <= destroyDistance)
        {
            Debug.Log("사람을 놓쳤습니다!");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnPersonMissed();
            }

            ClearMyDots();
            Destroy(gameObject);
            return;
        }

        float targetSpeed = maxMoveSpeed;
        if (distance <= slowDownDistance)
        {
            targetSpeed = maxMoveSpeed * (distance / slowDownDistance);
        }

        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector2 direction = (target.anchoredPosition - rectTransform.anchoredPosition).normalized;
        UpdateAnimation(direction);

        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition,
            target.anchoredPosition,
            currentMoveSpeed * Time.deltaTime
        );
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0) animator.SetInteger("Direction", 3); // 우
            else animator.SetInteger("Direction", 2); // 좌
        }
        else
        {
            if (direction.y > 0) animator.SetInteger("Direction", 1); // 상
            else animator.SetInteger("Direction", 0); // 하
        }
    }

    // ★ 파도에 맞았을 때 호출되는 함수
    public void HitByWave()
    {
        currentState = PersonState.Swept;

        // 1. 애니메이션 멈추거나 피격 상태로 전환 가능 (필요시 구현)
        // 2. 이 사람을 위해 생성되었던 모든 PathDot들을 깔끔하게 파괴!
        ClearMyDots();
    }

    // 내 경로에 있던 Dot들을 지워주는 헬퍼 함수
    private void ClearMyDots()
    {
        foreach (var dot in myDots)
        {
            if (dot != null)
            {
                Destroy(dot.gameObject);
            }
        }
        myDots.Clear();
    }
}