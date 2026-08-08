using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // 계속 위쪽으로 이동
        rb.MovePosition(
            rb.position +
            Vector2.up * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Trigger를 만나면 삭제
        if (other.CompareTag("BlockDestroyTrigger"))
        {
            Debug.Log("빨간 블록이 Trigger에 들어왔습니다!");

            Destroy(gameObject);
        }
    }
}