using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 500f;

    [Header("Destroy Trigger Tag")]
    [SerializeField] private string destroyTriggerTag = "BlockDestroyTrigger";

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // =========================================
        // 1. 계속 위쪽으로 이동
        // =========================================

        rectTransform.position +=
            Vector3.up * moveSpeed * Time.deltaTime;


        // =========================================
        // 2. Trigger 영역 검사
        // =========================================

        CheckTriggerOverlap();
    }


    private void CheckTriggerOverlap()
    {
        if (rectTransform == null)
            return;


        GameObject[] triggerObjects;

        try
        {
            triggerObjects =
                GameObject.FindGameObjectsWithTag(
                    destroyTriggerTag
                );
        }
        catch
        {
            Debug.LogError(
                "BlockDestroyTrigger Tag가 존재하지 않습니다."
            );

            return;
        }


        foreach (GameObject triggerObject in triggerObjects)
        {
            RectTransform triggerRect =
                triggerObject.GetComponent<RectTransform>();


            if (triggerRect == null)
                continue;


            // =========================================
            // 빨간 블록 영역
            // =========================================

            Bounds blockBounds =
                GetRectTransformBounds(rectTransform);


            // =========================================
            // Trigger 영역
            // =========================================

            Bounds triggerBounds =
                GetRectTransformBounds(triggerRect);


            // =========================================
            // 서로 겹치는지 확인
            // =========================================

            if (blockBounds.Intersects(triggerBounds))
            {
                Debug.Log(
                    "빨간 블록이 Trigger 영역에 들어왔습니다!"
                );


                Destroy(gameObject);

                return;
            }
        }
    }


    // =========================================================
    // RectTransform의 실제 World 영역 가져오기
    // =========================================================

    private Bounds GetRectTransformBounds(
        RectTransform rect)
    {
        Vector3[] corners =
            new Vector3[4];


        rect.GetWorldCorners(corners);


        Bounds bounds =
            new Bounds(
                corners[0],
                Vector3.zero
            );


        for (int i = 1; i < corners.Length; i++)
        {
            bounds.Encapsulate(
                corners[i]
            );
        }


        return bounds;
    }
}