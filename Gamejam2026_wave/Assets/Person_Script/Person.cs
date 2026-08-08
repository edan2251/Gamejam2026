using UnityEngine;

public class Person : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float acceleration = 3f;

    [Header("Slow Down")]
    [SerializeField] private float slowDownDistance = 2f;
    [SerializeField] private float destroyDistance = 0.1f;

    private float currentMoveSpeed = 0f;

    private Transform target;

    public void SetTarget(Transform target)
    {
        this.target = target;
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

