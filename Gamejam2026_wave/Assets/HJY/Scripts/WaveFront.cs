using System.Collections.Generic;
using UnityEngine;
using static Person;

public class WaveFront : MonoBehaviour
{
    private List<Person> sweptPersons = new List<Person>();

    [Header("HitBox Settings")]
    public RectTransform hitBox;
    private RectTransform myRectTransform;

    // ★ 목적지 이름들을 담아둘 리스트
    [HideInInspector]
    public List<string> targetWalls = new List<string>();

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
        if (hitBox == null) hitBox = myRectTransform;
    }

    public void Initialize(int size)
    {
        sweptPersons.Clear();
    }

    // ★ Spawn에서 목적지 이름들을 넘겨받는 함수
    public void SetTargetWalls(List<string> targets)
    {
        targetWalls = targets;
    }

    private void Update()
    {
        if (hitBox == null) return;

        Rect waveTipRect = GetWorldRect(hitBox);

        for (int i = Person.ActivePersons.Count - 1; i >= 0; i--)
        {
            Person person = Person.ActivePersons[i];
            if (person == null) continue;

            if (person.currentState == PersonState.Moving && !sweptPersons.Contains(person))
            {
                Rect personRect = GetWorldRect(person.rectTransform);

                if (waveTipRect.Overlaps(personRect))
                {
                    person.HitByWave();
                    person.transform.SetParent(this.transform, true);
                    sweptPersons.Add(person);

                    if (GameFeelManager.Instance != null)
                    {
                        GameFeelManager.Instance.TriggerHitStop();
                    }

                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlaySFX(SoundManager.Instance.hitImpactClip);
                    }
                }
            }
        }
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        float xMin = corners[0].x, xMax = corners[0].x, yMin = corners[0].y, yMax = corners[0].y;
        for (int i = 1; i < 4; i++)
        {
            if (corners[i].x < xMin) xMin = corners[i].x;
            if (corners[i].x > xMax) xMax = corners[i].x;
            if (corners[i].y < yMin) yMin = corners[i].y;
            if (corners[i].y > yMax) yMax = corners[i].y;
        }
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    // ★ 대망의 정산 함수 (다인승 파도 개별 판정 완벽 적용!)
    public void ProcessWaveEndScoreAndCleanup()
    {
        int scoreGained = sweptPersons.Count;

        if (scoreGained > 0)
        {
            if (DeathWallManager.Instance != null && DeathWallManager.Instance.activeWalls.Count > 0)
            {
                bool hitDeathWall = false;

                // 휩쓸려온 사람들을 한 명씩 개별적으로 검사합니다! (연대 책임 폐지)
                foreach (var person in sweptPersons)
                {
                    if (person == null) continue;

                    GameObject closestValidWall = null;
                    float minDistance = float.MaxValue;

                    // ★ 핵심: 전체 벽이 아니라, 파도가 날아온 '목적지 리스트(targetWalls)' 안에서만 찾습니다!
                    foreach (var wallName in targetWalls)
                    {
                        // 이름으로 DeathWallManager에 있는 12개 벽 중에서 실제 오브젝트를 찾아옵니다.
                        GameObject wallObj = DeathWallManager.Instance.allWalls.Find(w => w.name == wallName);
                        if (wallObj == null) continue;

                        // 그 벽과 이 사람 사이의 거리를 잽니다.
                        float dist = Vector2.Distance(person.transform.position, wallObj.transform.position);

                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closestValidWall = wallObj; // 사람이 실제로 도착한 정확한 한 칸!
                        }
                    }

                    // 이 사람이 최종적으로 밀려난 바로 그 '한 칸'이 현재 켜져있는 뜨거운 벽인가?
                    if (closestValidWall != null && DeathWallManager.Instance.activeWalls.Contains(closestValidWall))
                    {
                        hitDeathWall = true;
                        Debug.Log($"앗! 사람이 {closestValidWall.name} 뜨거운 벽에 정확히 꽂혔습니다!");
                        break; // 한 명이라도 빨간 벽에 닿았으면 즉시 사형!
                    }
                }

                // 사망자가 발생했다면 즉시 게임오버 처리
                if (hitDeathWall)
                {
                    if (UIManager.Instance != null) UIManager.Instance.GameOver();
                    foreach (var p in sweptPersons) { if (p != null) Destroy(p.gameObject); }
                    sweptPersons.Clear();
                    return;
                }
            }

            // --- 정상 득점 (아무도 빨간 벽에 안 닿았을 때만 실행) ---
            if (UIManager.Instance != null) UIManager.Instance.AddScore(scoreGained);

            if (GameFeelManager.Instance != null)
            {
                GameFeelManager.Instance.TriggerShake(1f + (scoreGained * 0.3f), transform.position);
            }

            if (DeathWallManager.Instance != null && UIManager.Instance != null)
            {
                DeathWallManager.Instance.TriggerWallTransition(UIManager.Instance.currentScore);
            }
        }

        foreach (var person in sweptPersons)
        {
            if (person != null) Destroy(person.gameObject);
        }
        sweptPersons.Clear();
    }
}