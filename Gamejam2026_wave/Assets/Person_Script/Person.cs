using UnityEngine;
using System.Collections.Generic;

public class Person : MonoBehaviour
{
    public enum PersonState { Moving, Swept }
    public PersonState currentState = PersonState.Moving;

    // ★ 핵심: 화면에 살아있는 모든 사람을 추적하는 명부 (물리 엔진 대체용)
    public static List<Person> ActivePersons = new List<Person>();

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

    private List<PathDot> myDots = new List<PathDot>();

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
    }

    // ★ 활성화될 때 명부에 등록
    private void OnEnable()
    {
        if (!ActivePersons.Contains(this)) ActivePersons.Add(this);
    }

    // ★ 비활성화/파괴될 때 명부에서 삭제
    private void OnDisable()
    {
        if (ActivePersons.Contains(this)) ActivePersons.Remove(this);
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

            myDots.Add(dot);
        }
    }

    private void Update()
    {
        if (target == null) return;

        if (currentState == PersonState.Swept) return;

        float distance = Vector2.Distance(rectTransform.anchoredPosition, target.anchoredPosition);

        if (distance <= destroyDistance)
        {
            Debug.Log("사람을 놓쳤습니다!");
            if (UIManager.Instance != null) UIManager.Instance.OnPersonMissed();

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
            if (direction.x > 0) animator.SetInteger("Direction", 3);
            else animator.SetInteger("Direction", 2);
        }
        else
        {
            if (direction.y > 0) animator.SetInteger("Direction", 1);
            else animator.SetInteger("Direction", 0);
        }
    }

    public void HitByWave()
    {
        currentState = PersonState.Swept;
        ClearMyDots();
    }

    private void ClearMyDots()
    {
        foreach (var dot in myDots)
        {
            if (dot != null) Destroy(dot.gameObject);
        }
        myDots.Clear();
    }
}