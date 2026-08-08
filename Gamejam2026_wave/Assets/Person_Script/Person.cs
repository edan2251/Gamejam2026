using UnityEngine;

public class Person : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float acceleration = 3f;

    [Header("Slow Down")]
    [SerializeField] private float slowDownDistance = 2f;
    [SerializeField] private float destroyDistance = 0.1f;

    [Header("Path")]
    [SerializeField] private PathDot pathDotPrefab;
    [SerializeField] private float dotSpacing = 0.5f;

    private float currentMoveSpeed = 0f;

    private Transform target;

    public void SetTarget(Transform target)
    {
        this.target = target;

        CreatePath();
    }

    private void CreatePath()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            target.position
        );

        int dotCount = Mathf.FloorToInt(
            distance / dotSpacing
        );

        for (int i = 1; i <= dotCount; i++)
        {
            float ratio = (float)i / dotCount;

            Vector2 position = Vector2.Lerp(
                transform.position,
                target.position,
                ratio
            );

            PathDot dot = Instantiate(
                pathDotPrefab,
                position,
                Quaternion.identity
            );

            dot.SetPerson(transform);
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        float distance = Vector2.Distance(
           transform.position,
           target.position
       );

        // 타겟에 도착
        if (distance <= destroyDistance)
        {
            Destroy(gameObject);
            return;
        }

        // 감속해야 할 거리인지 확인
        float targetSpeed = maxMoveSpeed;

        if (distance <= slowDownDistance)
        {
            targetSpeed = maxMoveSpeed * (
                distance / slowDownDistance
            );
        }

        // 현재 속도를 목표 속도에 맞춰 변화
        currentMoveSpeed = Mathf.MoveTowards(
            currentMoveSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        // 타겟 방향
        Vector2 direction = (
            target.position - transform.position
        ).normalized;

        // 이동
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            currentMoveSpeed * Time.deltaTime
        );
    }
}

