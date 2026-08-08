using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Controllers")]
    public TweenController tweenController;
    public ManaManager manaManager;

    [Header("Data")]
    public int highScore = 99; //나중에 최대점수 변수로 변경해야함.

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

    void Start()
    {
        if (tweenController != null)
        {
            tweenController.SetInitialState(this);
        }
        else
        {
            Debug.LogError("컨트롤러 연결 확인");
        }
    }

    // 시작 버튼을 누르거나 화면을 터치했을 때 실행할 함수
    public void OnGameStart()
    {
        if (tweenController != null)
        {
            tweenController.PlayStartTransition(this);
            manaManager.PlayIntroWaveAnimation();
            startButton.SetActive(false);
        }
    }
}