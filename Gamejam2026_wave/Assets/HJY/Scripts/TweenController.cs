using UnityEngine;
using DG.Tweening;

public class TweenController : MonoBehaviour
{
    [Header("Transition Settings")]
    public float transitionDuration = 1.0f;

    [Header("Grid Animation")]
    public Vector3 gridInitialScale = new Vector3(0.7f, 0.7f, 1f);
    public float gridInitialAlpha = 0.5f;

    [Header("Title Animation")]
    public float titleTargetScale = 0.5f;

    [Header("Score Animation")]
    public float scoreTargetPosY = 400f;
    [HideInInspector] public float scoreInitialPosY; // 시작 전 원래 위치 저장용

    [Header("Blinking Prompt")]
    public float blinkDuration = 0.8f;
    public float blinkMinAlpha = 0.2f;

    public void SetInitialState(UIManager ui)
    {
        // 원래 위치 저장해두기 (나중에 돌아오기 위해)
        scoreInitialPosY = ui.scoreRect.anchoredPosition.y;

        ui.titleRect.localScale = Vector3.one;
        ui.titleCanvasGroup.alpha = 1f;

        ui.maxscoreTextRect.localScale = Vector3.one;
        ui.maxscoreTextCanvasGroup.alpha = 1f;

        ui.gridRect.localScale = gridInitialScale;
        ui.gridCanvasGroup.alpha = gridInitialAlpha;

        ui.startPromptTxt.alpha = 1f;
        ui.startPromptTxt.DOFade(blinkMinAlpha, blinkDuration).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);

        ui.scoreTxt.text = ui.highScore.ToString();
    }

    public void PlayStartTransition(UIManager ui)
    {
        Sequence startSequence = DOTween.Sequence();

        ui.startPromptTxt.DOKill();
        startSequence.Join(ui.startPromptTxt.DOFade(0f, transitionDuration * 0.5f));

        startSequence.Join(ui.gridRect.DOScale(1f, transitionDuration).SetEase(Ease.OutBack));
        startSequence.Join(ui.gridCanvasGroup.DOFade(1f, transitionDuration * 0.9f));

        startSequence.Join(ui.titleRect.DOScale(titleTargetScale, transitionDuration).SetEase(Ease.InBack));
        startSequence.Join(ui.titleCanvasGroup.DOFade(0f, transitionDuration * 0.5f));

        startSequence.Join(ui.maxscoreTextRect.DOScale(titleTargetScale, transitionDuration).SetEase(Ease.InBack));
        startSequence.Join(ui.maxscoreTextCanvasGroup.DOFade(0f, transitionDuration * 0.5f));

        startSequence.Join(ui.scoreRect.DOAnchorPosY(scoreTargetPosY, transitionDuration * 0.7f).SetEase(Ease.InOutCubic));

        int currentDisplayScore = ui.highScore;
        startSequence.Join(DOTween.To(
            () => currentDisplayScore,
            x => {
                currentDisplayScore = x;
                ui.scoreTxt.text = currentDisplayScore.ToString("D2");
            },
            0,
            transitionDuration * 0.35f
        ).SetEase(Ease.OutQuad));

        startSequence.OnComplete(() =>
        {
            Debug.Log("DOTween 시작 연출 종료!");
        });
    }

    // ★ 게임 오버 시 실행되는 역재생 애니메이션
    public void PlayGameOverTransition(UIManager ui)
    {
        Sequence overSequence = DOTween.Sequence();

        // 1. 점수 텍스트 원래 자리로 내리기
        overSequence.Join(ui.scoreRect.DOAnchorPosY(scoreInitialPosY, transitionDuration * 0.7f).SetEase(Ease.InOutCubic));

        // 2. 타이틀 & 최대 점수 텍스트 다시 커지면서 등장
        overSequence.Join(ui.titleRect.DOScale(1f, transitionDuration).SetEase(Ease.OutBack));
        overSequence.Join(ui.titleCanvasGroup.DOFade(1f, transitionDuration * 0.5f));

        overSequence.Join(ui.maxscoreTextRect.DOScale(1f, transitionDuration).SetEase(Ease.OutBack));
        overSequence.Join(ui.maxscoreTextCanvasGroup.DOFade(1f, transitionDuration * 0.5f));

        // 3. 그리드 다시 작아지고 투명해지기
        overSequence.Join(ui.gridRect.DOScale(gridInitialScale, transitionDuration).SetEase(Ease.InBack));
        overSequence.Join(ui.gridCanvasGroup.DOFade(gridInitialAlpha, transitionDuration * 0.9f));

        // 연출이 모두 끝나면
        overSequence.OnComplete(() =>
        {
            // 블링킹 텍스트(시작하려면 터치하세요) 다시 재생
            ui.startPromptTxt.alpha = 1f;
            ui.startPromptTxt.DOFade(blinkMinAlpha, blinkDuration).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);

            // 텍스트를 최고 점수로 업데이트
            ui.scoreTxt.text = ui.highScore.ToString();

            // 버튼 활성화하여 다시 시작할 수 있게 만듦
            ui.startButton.SetActive(true);
            Debug.Log("게임 오버 연출 종료! 다시 시작 대기 중");
        });
    }
}