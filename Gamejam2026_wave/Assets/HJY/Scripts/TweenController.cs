using UnityEngine;
using DG.Tweening;

public class TweenController : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("전체 애니메이션 진행 시간")]
    public float transitionDuration = 1.0f;

    [Header("Grid Animation")]
    [Tooltip("그리드 초기 크기")]
    public Vector3 gridInitialScale = new Vector3(0.7f, 0.7f, 1f);
    [Tooltip("그리드 초기 투명도")]
    public float gridInitialAlpha = 0.5f;

    [Header("Title Animation")]
    [Tooltip("타이틀이 작아질 목표 크기")]
    public float titleTargetScale = 0.5f;

    [Header("Score Animation")]
    [Tooltip("점수 텍스트가 올라갈 목표 Y 좌표")]
    public float scoreTargetPosY = 400f;

    [Header("Blinking Prompt")]
    [Tooltip("깜빡임 애니메이션 주기 (시간)")]
    public float blinkDuration = 0.8f;
    [Tooltip("깜빡일 때 최소 투명도")]
    public float blinkMinAlpha = 0.2f;

    public void SetInitialState(UIManager ui)
    {
        //타이틀 초기화
        ui.titleRect.localScale = Vector3.one;
        ui.titleCanvasGroup.alpha = 1f;

        //그리드 초기화(작아지게끔)
        ui.gridRect.localScale = gridInitialScale;
        ui.gridCanvasGroup.alpha = gridInitialAlpha;

        //블링킹 효과
        ui.startPromptTxt.alpha = 1f;
        ui.startPromptTxt.DOFade(blinkMinAlpha, blinkDuration).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);

        //점수 최대점수로 초기화
        ui.scoreTxt.text = ui.highScore.ToString();
    }

    public void PlayStartTransition(UIManager ui)
    {
        Sequence startSequence = DOTween.Sequence();

        //블링킹텍스트 정상화
        ui.startPromptTxt.DOKill();
        startSequence.Join(ui.startPromptTxt.DOFade(0f, transitionDuration * 0.5f));

        //그리드 키우기
        startSequence.Join(ui.gridRect.DOScale(1f, transitionDuration).SetEase(Ease.OutBack));
        startSequence.Join(ui.gridCanvasGroup.DOFade(1f, transitionDuration * 0.9f));

        //타이틀 텍스트 숨기기
        startSequence.Join(ui.titleRect.DOScale(titleTargetScale, transitionDuration).SetEase(Ease.InBack));
        startSequence.Join(ui.titleCanvasGroup.DOFade(0f, transitionDuration * 0.5f));

        //최대점수 텍스트 숨기기
        startSequence.Join(ui.maxscoreTextRect.DOScale(titleTargetScale, transitionDuration).SetEase(Ease.InBack));
        startSequence.Join(ui.maxscoreTextCanvasGroup.DOFade(0f, transitionDuration * 0.5f));

        //점수 텍스트 상승
        startSequence.Join(ui.scoreRect.DOAnchorPosY(scoreTargetPosY, transitionDuration * 0.7f).SetEase(Ease.InOutCubic));

        //점수 텍스트 00으로 초기화
        int currentDisplayScore = ui.highScore;
        startSequence.Join(DOTween.To(
            () => currentDisplayScore,      //시작값 = 최대점수
            x => {
                currentDisplayScore = x;
                ui.scoreTxt.text = currentDisplayScore.ToString("D2"); //D2 = 00으로 표시
            },
            0,                              // 목표 값 : 0점
            transitionDuration * 0.35f              // 걸리는 시간 : 이동 시간과 똑같게
        ).SetEase(Ease.OutQuad));           // 숫자가 내려갈수록 살짝 느려지는 효과

        startSequence.OnComplete(() =>
        {
            Debug.Log("DOTween 연출 종료! 게임 로직 시작");
            //게임시작로직
        });
    }
}