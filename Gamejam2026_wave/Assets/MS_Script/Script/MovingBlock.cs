using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;

    private Vector2 moveDirection = Vector2.up;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        rb.MovePosition(
            rb.position +
            moveDirection *
            moveSpeed *
            Time.fixedDeltaTime
        );
    }


    // =========================================================
    // 이동 방향 설정
    // =========================================================

    public void SetMoveDirection(
        Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0f)
            return;

        moveDirection =
            direction.normalized;
    }


    // =========================================================
    // 이동 속도 설정
    // =========================================================

    public void SetMoveSpeed(
        float speed)
    {
        moveSpeed = speed;
    }


    // =========================================================
    // Trigger
    // =========================================================

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (other.CompareTag(
            "BlockDestroyTrigger"))
        {
            Debug.Log(
                "블록이 Trigger에 들어왔습니다!"
            );

            Destroy(gameObject);
        }
    }
}