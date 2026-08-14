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

    public bool isGameActive { get; private set; } = false;

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

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Start()
    {
        SetSpawnersActive(false);
        if (tweenController != null) tweenController.SetInitialState(this);
    }

    public void OnGameStart()
    {
        if (isGameActive) return;
        isGameActive = true;

        StopAllCoroutines();
        Time.timeScale = 1f;

        // ★ 안전장치: 유저가 2초 전에 시작 버튼을 광클했을 때 BGM이 작아진 채로 멈추는 것 방지
        if (SoundManager.Instance != null) SoundManager.Instance.RestoreBGM();

        currentScore = 0;
        currentMissedCount = 0;
        displayedScoreFloat = 0f;

        if (scoreTxt != null)
        {
            scoreTxt.transform.DOComplete();
            scoreTxt.DOComplete();
            scoreTxt.color = Color.white;
            scoreTxt.transform.localScale = Vector3.one;
            scoreTxt.text = "00";
        }

        if (startPromptTxt != null) startPromptTxt.gameObject.SetActive(false);
        if (startButton != null) startButton.SetActive(false);

        if (tweenController != null)
        {
            tweenController.PlayStartTransition(this);
        }

        if (manaManager != null) manaManager.PlayIntroWaveAnimation();
        SetSpawnersActive(true);

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

        UpdateScoreUI(amount);
    }

    public void OnPersonMissed()
    {
        if (!isGameActive) return;

        currentMissedCount++;
        Debug.Log($"놓친 사람 수: {currentMissedCount} / {maxMisses}");

        if (currentMissedCount >= maxMisses)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;

        Debug.Log("게임 오버!");

        SetSpawnersActive(false);

        if (GameFeelManager.Instance != null)
        {
            GameFeelManager.Instance.TriggerGameOverFeel(); // T=0: 플래시 번쩍임 + 화면 진동
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

        // =========================================================
        // ★ [동시 실행 구역] 사망과 '동시에' 모든 UI를 한꺼번에 복구시킵니다!
        // =========================================================

        // 1. 마나 UI 차오름
        if (manaManager != null) manaManager.ResetMana();

        // 2. 블링킹 텍스트 및 시작 버튼 즉시 켜기
        if (startPromptTxt != null) startPromptTxt.gameObject.SetActive(true);
        if (startButton != null) startButton.SetActive(true);

        // 3. 타이틀 및 최고점수 UI 트윈 애니메이션 즉시 실행
        if (tweenController != null) tweenController.PlayGameOverTransition(this);

        // =========================================================

        SoundManager.Instance.DuckBGM();
        StartCoroutine(GameOverSequence()); // 사운드 처리를 위해 남겨둠
    }

    private void UpdateScoreUI(int gainedAmount)
    {
        if (scoreTxt == null) return;

        float pitch = 1f + (gainedAmount * 0.15f);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.scoreChimeClip, 0.8f, pitch);

        float rollDuration = Mathf.Clamp(gainedAmount * 0.15f, 0.2f, 0.6f);

        DOTween.Kill("ScoreRoll");
        DOTween.To(() => displayedScoreFloat, x =>
        {
            displayedScoreFloat = x;
            scoreTxt.text = Mathf.FloorToInt(displayedScoreFloat).ToString("D2");
        }, currentScore, rollDuration)
        .SetId("ScoreRoll")
        .SetEase(Ease.OutQuad);

        int intensity = Mathf.Clamp(gainedAmount, 1, 5);
        float punchScale = 0.2f + (intensity * 0.15f);
        float punchDuration = 0.2f + (intensity * 0.05f);
        int vibrato = 3 + intensity;

        Color targetColor = Color.white;
        switch (intensity)
        {
            case 1: targetColor = Color.white; break;
            case 2: targetColor = new Color(1f, 0.9f, 0.2f); break;
            case 3: targetColor = new Color(1f, 0.5f, 0f); break;
            case 4: targetColor = new Color(1f, 0.2f, 0.2f); break;
            case 5: targetColor = new Color(0.8f, 0.2f, 1f); break;
        }

        scoreTxt.transform.DOComplete();
        scoreTxt.transform.localScale = Vector3.one;
        scoreTxt.transform.DOPunchScale(new Vector3(punchScale, punchScale, 0f), punchDuration, vibrato, 1f);

        scoreTxt.DOComplete();

        if (intensity == 1)
        {
            scoreTxt.color = new Color(0.8f, 0.8f, 0.8f);
        }
        else
        {
            scoreTxt.color = targetColor;
        }

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
        SoundManager.Instance.PlayGameOverSound();

        // ★ 연출은 이미 위에서 다 끝났고, 사운드 볼륨만 2초 뒤에 스르륵 올립니다.
        yield return new WaitForSecondsRealtime(2.0f);
        SoundManager.Instance.RestoreBGM();
    }
}