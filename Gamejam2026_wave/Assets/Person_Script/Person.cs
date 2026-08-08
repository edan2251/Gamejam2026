using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    private Transform target;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void Update()
    {
        if (target == null)
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );
    }
}
