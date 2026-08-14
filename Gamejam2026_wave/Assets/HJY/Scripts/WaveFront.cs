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

        Rect myRect = GetWorldRect(myRectTransform);

        for (int i = Person.ActivePersons.Count - 1; i >= 0; i--)
        {
            Person person = Person.ActivePersons[i];

            if (person == null) continue;

            if (person.currentState == PersonState.Moving && !sweptPersons.Contains(person))
            {
                Rect personRect = GetWorldRect(person.rectTransform);

                if (myRect.Overlaps(personRect))
                {
                    person.HitByWave();
                    person.transform.SetParent(this.transform);
                    sweptPersons.Add(person);

                    // ★ [수정 완료] 부딪히는 순간에는 흔들림이 아니라 역경직(HitStop)을 발생시킵니다!
                    if (GameFeelManager.Instance != null)
                    {
                        GameFeelManager.Instance.TriggerHitStop();
                    }
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
            if (UIManager.Instance != null) UIManager.Instance.AddScore(scoreGained);

            if (GameFeelManager.Instance != null)
            {
                // ★ 수정됨: 현재 파도의 위치(transform.position)를 충격파의 중심으로 같이 넘겨줍니다!
                GameFeelManager.Instance.TriggerShake(1f + (scoreGained * 0.3f), transform.position);
            }
        }

        foreach (var person in sweptPersons)
        {
            if (person != null) Destroy(person.gameObject);
        }
        sweptPersons.Clear();
    }
}