using System.Collections.Generic;
using UnityEngine;
using static Person;

public class WaveFront : MonoBehaviour
{
    private List<Person> sweptPersons = new List<Person>();
    private RectTransform myRectTransform;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(int size)
    {
        sweptPersons.Clear();
    }

    // ★ 매 프레임마다 UI 좌표를 수학적으로 검사합니다!
    private void Update()
    {
        if (myRectTransform == null) return;

        // 1. 내 파도 히트박스의 실제 화면 상 영역(사각형)을 구합니다.
        Rect myRect = GetWorldRect(myRectTransform);

        // 2. 현재 화면에 살아있는 모든 사람을 순회하며 검사합니다. (역순으로 순회하여 중간 삭제 에러 방지)
        for (int i = Person.ActivePersons.Count - 1; i >= 0; i--)
        {
            Person person = Person.ActivePersons[i];

            if (person == null) continue;

            // 아직 안 쓸려간 사람인지 확인
            if (person.currentState == PersonState.Moving && !sweptPersons.Contains(person))
            {
                // 3. 사람의 실제 화면 상 영역(사각형)을 구합니다.
                Rect personRect = GetWorldRect(person.rectTransform);

                // 4. 두 사각형이 겹쳤는지 교집합(Overlaps) 검사!
                if (myRect.Overlaps(personRect))
                {
                    person.HitByWave();
                    person.transform.SetParent(this.transform);
                    sweptPersons.Add(person);

                    Debug.Log($"[{gameObject.name}] UI 좌표 충돌 감지! {person.gameObject.name} 휩쓸림.");
                }
            }
        }
    }

    // UI 객체(RectTransform)의 4개 꼭짓점을 구해서 완벽한 수학적 사각형(Rect)을 만들어주는 헬퍼 함수
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        float xMin = corners[0].x;
        float xMax = corners[0].x;
        float yMin = corners[0].y;
        float yMax = corners[0].y;

        for (int i = 1; i < 4; i++)
        {
            if (corners[i].x < xMin) xMin = corners[i].x;
            if (corners[i].x > xMax) xMax = corners[i].x;
            if (corners[i].y < yMin) yMin = corners[i].y;
            if (corners[i].y > yMax) yMax = corners[i].y;
        }

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public void ProcessWaveEndScoreAndCleanup()
    {
        int scoreGained = sweptPersons.Count;

        if (scoreGained > 0)
        {
            Debug.Log($"파도 연출 종료! 득점: +{scoreGained}명 밀어냄");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddScore(scoreGained);
            }
        }

        foreach (var person in sweptPersons)
        {
            if (person != null) Destroy(person.gameObject);
        }
        sweptPersons.Clear();
    }
}