using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Controllers")]
    public TweenController tweenController;
    public ManaManager manaManager;

    [Header("Data")]
    public int highScore = 0;
    private int currentScore = 0;

    [Header("Game Over Settings")]
    public int maxMisses = 10;
    private int currentMissedCount = 0;

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

            tweenController.PlayStartTransition(this);
            manaManager.PlayIntroWaveAnimation();
            startButton.SetActive(false);
            SetSpawnersActive(true);
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (currentScore > highScore)
        {
            highScore = currentScore;

            // ★ 1. 기기에 최고 점수 영구 저장! (앱을 껐다 켜도 유지됨)
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        UpdateScoreUI();
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

    private void GameOver()
    {
        Debug.Log("게임 오버!");

        SetSpawnersActive(false);

        Person[] allPersons = UnityEngine.Object.FindObjectsByType<Person>(FindObjectsInactive.Include);
        foreach (Person p in allPersons) Destroy(p.gameObject);

        WaveFront[] allWaves = UnityEngine.Object.FindObjectsByType<WaveFront>(FindObjectsInactive.Include);
        foreach (WaveFront w in allWaves)
        {
            if (w.transform.parent != null) Destroy(w.transform.parent.gameObject);
            else Destroy(w.gameObject);
        }

        // ★ 3. 화면에 흩어져 있는 쩜쩜쩜(PathDot)들도 모조리 찾아서 파괴!
        PathDot[] allDots = UnityEngine.Object.FindObjectsByType<PathDot>(FindObjectsInactive.Include);
        foreach (PathDot d in allDots) Destroy(d.gameObject);

        // 2번 문제 해결을 위해 마나 UI를 초기 위치로 되돌리는 함수 호출
        if (manaManager != null)
        {
            manaManager.ResetMana();
        }

        if (tweenController != null)
        {
            tweenController.PlayGameOverTransition(this);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreTxt != null) scoreTxt.text = currentScore.ToString("D2");
    }

    private void SetSpawnersActive(bool isActive)
    {
        if (personSpawners == null) return;
        foreach (GameObject spawner in personSpawners)
        {
            if (spawner != null) spawner.SetActive(isActive);
        }
    }
}