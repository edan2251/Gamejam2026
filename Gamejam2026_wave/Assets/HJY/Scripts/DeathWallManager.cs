using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // ★ 연출을 위해 DOTween 추가!

[System.Serializable]
public struct WallDifficulty
{
    public int targetScore;
    public int wallCount;
}

public class DeathWallManager : MonoBehaviour
{
    public static DeathWallManager Instance { get; private set; }

    [Header("Red Wall UI Objects (12개 테두리)")]
    public List<GameObject> allWalls;

    [Header("Difficulty Settings")]
    public List<WallDifficulty> difficultyStages;

    [Header("Transition Settings (연출 타이밍)")]
    public float delayBeforeTransition = 0.5f; // 점수 확인을 위해 대기하는 시간
    public float scaleOutDuration = 0.3f;      // 벽이 사라지는 시간
    public float scaleInDuration = 0.4f;       // 새 벽이 나타나는 시간

    [HideInInspector]
    public List<GameObject> activeWalls = new List<GameObject>();

    private int currentWallCount = 1;
    private bool isGameActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        isGameActive = true;
        // 시작할 때는 지연 없이 바로 띄웁니다.
        StartCoroutine(TransitionRoutine(0, true));
    }

    public void EndGame()
    {
        isGameActive = false;
        StopAllCoroutines();
        foreach (var w in allWalls)
        {
            w.transform.DOKill();
            w.SetActive(false);
        }
        activeWalls.Clear();
    }

    // ★ WaveFront에서 점수를 낸 직후 호출할 함수
    public void TriggerWallTransition(int currentTotalScore)
    {
        if (!isGameActive) return;
        StartCoroutine(TransitionRoutine(currentTotalScore, false));
    }

    private IEnumerator TransitionRoutine(int currentTotalScore, bool isImmediate)
    {
        // 1. 여운 대기 (시작할 때가 아니면 0.5초 대기하여 유저가 점수를 확인하게 함)
        if (!isImmediate)
        {
            yield return new WaitForSeconds(delayBeforeTransition);
        }

        // 2. 기존 벽 퇴장 연출 (작아지면서 사라짐)
        if (activeWalls.Count > 0)
        {
            foreach (var wall in activeWalls)
            {
                // 약간 탄력있게(InBack) 쪼그라듦
                wall.transform.DOScale(Vector3.zero, scaleOutDuration).SetEase(Ease.InBack);
            }
            yield return new WaitForSeconds(scaleOutDuration); // 다 작아질 때까지 대기
        }

        // --- 내부 로직 갱신 ---
        int targetCount = 1;
        if (difficultyStages != null)
        {
            foreach (var stage in difficultyStages)
            {
                if (currentTotalScore >= stage.targetScore)
                {
                    targetCount = stage.wallCount;
                }
            }
        }
        currentWallCount = targetCount;

        activeWalls.Clear();
        foreach (var w in allWalls)
        {
            w.SetActive(false);
            w.transform.localScale = Vector3.zero; // 다음 등장을 위해 스케일을 0으로 초기화
        }

        List<GameObject> tempPool = new List<GameObject>(allWalls);

        for (int i = 0; i < currentWallCount; i++)
        {
            if (tempPool.Count == 0) break;

            int randIndex = Random.Range(0, tempPool.Count);
            GameObject selectedWall = tempPool[randIndex];

            activeWalls.Add(selectedWall);
            tempPool.RemoveAt(randIndex);
        }

        // 3. 새로운 벽 등장 연출 (커지면서 나타남)
        foreach (var wall in activeWalls)
        {
            wall.SetActive(true);
            // 탄력있게(OutBack) 띠용! 하고 튀어나옴
            wall.transform.DOScale(Vector3.one, scaleInDuration).SetEase(Ease.OutBack);
        }

        Debug.Log($"점수 {currentTotalScore}점: 벽 {currentWallCount}개로 찰지게 재배치 완료!");
    }
}