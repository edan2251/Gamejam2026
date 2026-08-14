using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Controllers")]
    public TweenController tweenController;
    public ManaManager manaManager;

    [Header("Data")]
    public int highScore = 0;
    public int currentScore = 0;

    [Header("Game Over Settings")]
    public int maxMisses = 10;
    private int currentMissedCount = 0;

    private float displayedScoreFloat = 0f;

    [Header("Spawners")]
    public GameObject[] personSpawners;

    [Header("UI")]
    public RectTransform titleRect;
    public CanvasGroup titleCanvasGroup;

    public RectTransform maxscoreTextRect;
    public CanvasGroup maxscoreTextCanvasGroup;

    public RectTransform gridRect;
    public CanvasGroup gridCanvasGroup;

    public RectTransform scoreRect;
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI startPromptTxt;

    public GameObject startButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // ★ 1. 최고 점수 불러오기 (저장된 기록이 없으면 기본값 99)
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Start()
    {
        SetSpawnersActive(false);
        if (tweenController != null) tweenController.SetInitialState(this);
    }

    public void OnGameStart()
    {
        if (tweenController != null)
        {
            currentScore = 0;
            currentMissedCount = 0;

            displayedScoreFloat = 0f;
            if (scoreTxt != null) scoreTxt.text = "00";

            tweenController.PlayStartTransition(this);
            manaManager.PlayIntroWaveAnimation();
            startButton.SetActive(false);
            SetSpawnersActive(true);
        }

        if (DeathWallManager.Instance != null) DeathWallManager.Instance.StartGame();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // ★ 기존: UpdateScoreUI(); 
        // ★ 수정: 얻은 점수를 통째로 넘겨줍니다!
        UpdateScoreUI(amount);
    }

    public void OnPersonMissed()
    {
        currentMissedCount++;
        Debug.Log($"놓친 사람 수: {currentMissedCount} / {maxMisses}");

        if (currentMissedCount >= maxMisses)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        Debug.Log("게임 오버!");

        SetSpawnersActive(false);

        if (GameFeelManager.Instance != null)
        {
            GameFeelManager.Instance.TriggerGameOverFeel();
        }

        if (DeathWallManager.Instance != null) DeathWallManager.Instance.EndGame();

        Person[] allPersons = UnityEngine.Object.FindObjectsByType<Person>(FindObjectsInactive.Exclude);
        foreach (Person p in allPersons) Destroy(p.gameObject);

        WaveFront[] allWaves = UnityEngine.Object.FindObjectsByType<WaveFront>(FindObjectsInactive.Exclude);
        foreach (WaveFront w in allWaves)
        {
            if (w.transform.parent != null) Destroy(w.transform.parent.gameObject);
            else Destroy(w.gameObject);
        }

        PathDot[] allDots = UnityEngine.Object.FindObjectsByType<PathDot>(FindObjectsInactive.Exclude);
        foreach (PathDot d in allDots) Destroy(d.gameObject);

        // ★ 수정 완료: 떨어지는 연출 대신, 마나를 복구하며 차오르게 만듭니다.
        if (manaManager != null)
        {
            manaManager.ResetMana();
        }

        if (tweenController != null)
        {
            tweenController.PlayGameOverTransition(this);
        }

        // ★ 1. BGM 볼륨 낮추기 (Ducking)
        SoundManager.Instance.DuckBGM();

        // ★ 2. 효과음 재생 후 복구 코루틴 실행
        StartCoroutine(GameOverSequence());
    }

    private void UpdateScoreUI(int gainedAmount)
    {
        if (scoreTxt == null) return;

        float pitch = 1f + (gainedAmount * 0.15f);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.scoreChimeClip, 0.8f, pitch);

        // --- 1. 점수가 '촤자작' 올라가는 롤링 애니메이션 ---
        // 1점이면 0.2초, 4~5점이면 최대 0.6초에 걸쳐 타다닥! 하고 올라갑니다.
        float rollDuration = Mathf.Clamp(gainedAmount * 0.15f, 0.2f, 0.6f);

        DOTween.Kill("ScoreRoll"); // 이전 롤링이 안 끝났다면 캔슬
        DOTween.To(() => displayedScoreFloat, x =>
        {
            displayedScoreFloat = x;
            // 소수점을 내림해서 D2(01, 02) 형식의 텍스트로 보여줍니다.
            scoreTxt.text = Mathf.FloorToInt(displayedScoreFloat).ToString("D2");
        }, currentScore, rollDuration)
        .SetId("ScoreRoll")
        .SetEase(Ease.OutQuad);

        // --- 2. 획득 점수에 비례한 강도(Intensity) 계산 ---
        // 최대치는 5점으로 제한합니다.
        int intensity = Mathf.Clamp(gainedAmount, 1, 5);

        // 점수가 높을수록 더 크게 부풀고(Scale), 더 오래 흔들립니다(Vibrato).
        float punchScale = 0.2f + (intensity * 0.15f); // 1점: 0.35배, 5점: 0.95배 팽창!
        float punchDuration = 0.2f + (intensity * 0.05f); // 1점: 0.25초, 5점: 0.45초
        int vibrato = 3 + intensity;

        // --- 3. 미감을 살린 단계별 콤보 색상 변화 (Heat Scale) ---
        Color targetColor = Color.white;
        switch (intensity)
        {
            case 1: targetColor = Color.white; break;                          // 1점: 깔끔한 순백색
            case 2: targetColor = new Color(1f, 0.9f, 0.2f); break;            // 2점: 빛나는 레몬 노란색
            case 3: targetColor = new Color(1f, 0.5f, 0f); break;              // 3점: 뜨거운 텐션의 오렌지색
            case 4: targetColor = new Color(1f, 0.2f, 0.2f); break;            // 4점: 경고등 같은 크림슨 레드
            case 5: targetColor = new Color(0.8f, 0.2f, 1f); break;            // 5점: 세계관 붕괴 급의 마젠타(보라)색
        }

        // --- 4. 팝 앤 플래시 (크기 팽창 + 색상 연출) ---
        scoreTxt.transform.DOComplete();
        scoreTxt.transform.localScale = Vector3.one;
        scoreTxt.transform.DOPunchScale(new Vector3(punchScale, punchScale, 0f), punchDuration, vibrato, 1f);

        scoreTxt.DOComplete(); // 색상 애니메이션 중복 방지

        if (intensity == 1)
        {
            // 1점일 때는 살짝 회색으로 변했다가 돌아오며 소소한 타격감만 줌
            scoreTxt.color = new Color(0.8f, 0.8f, 0.8f);
        }
        else
        {
            // 2점 이상부터는 준비한 화려한 컬러를 확! 덮어씌움
            scoreTxt.color = targetColor;
        }

        // 모든 색상은 팽창이 끝날 즈음(punchDuration + 0.3초) 스르륵 깨끗한 흰색으로 되돌아옵니다.
        scoreTxt.DOColor(Color.white, punchDuration + 0.3f);
    }

    private void SetSpawnersActive(bool isActive)
    {
        if (personSpawners == null) return;
        foreach (GameObject spawner in personSpawners)
        {
            if (spawner != null) spawner.SetActive(isActive);
        }
    }

    private System.Collections.IEnumerator GameOverSequence()
    {
        // 효과음 재생
        SoundManager.Instance.PlayGameOverSound();

        // 효과음 길이만큼 대기 (만약 Sizzle 사운드가 2초라면 2초 대기)
        yield return new WaitForSecondsRealtime(2.0f);

        // ★ 3. BGM 볼륨 복구
        SoundManager.Instance.RestoreBGM();

        // 이후 남은 오버 연출 실행
        if (tweenController != null) tweenController.PlayGameOverTransition(this);
    }
}