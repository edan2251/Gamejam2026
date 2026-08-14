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

                    // ★ [타격감 연출 1 & 2] 역경직과 플로팅 텍스트 발생!
                    if (GameFeelManager.Instance != null)
                    {
                        GameFeelManager.Instance.TriggerHitStop();

                        // 현재까지 모은 콤보 수치로 텍스트 띄우기 (예: "1 Combo!", "2 Combo!")
                        GameFeelManager.Instance.SpawnFloatingText($"+{sweptPersons.Count}", person.transform.position);
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

            // ★ [타격감 연출 3] 파도가 끝날 때 점수를 정산하며 카메라 흔들기!
            // 많이 쓸어담았을수록 화면이 더 강하게 흔들립니다 (scoreGained * 0.3f)
            if (GameFeelManager.Instance != null)
            {
                GameFeelManager.Instance.TriggerCameraShake(1f + (scoreGained * 0.3f));
            }
        }

        foreach (var person in sweptPersons)
        {
            if (person != null) Destroy(person.gameObject);
        }
        sweptPersons.Clear();
    }
}