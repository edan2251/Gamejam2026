using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // ★ URP 효과를 코드로 제어하기 위해 추가!

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance { get; private set; }

    [Header("Hit Stop Settings")]
    public float hitStopDuration = 0.05f;

    [Header("Shake Target")]
    public RectTransform targetToShake;

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 50f; // 조금 더 폭발적으로(50) 올렸습니다.
    public int shakeVibrato = 15;

    [Header("Distortion Settings")]
    public Volume distortionVolume;
    public float distortionDuration = 0.5f; // 여운을 위해 살짝 늘림

    [Header("Game Over Flash Settings")]
    public UnityEngine.UI.Image flashOverlay; // 배경 캔버스에 넣을 번쩍이는 이미지
    public float flashInDuration = 0.05f;
    public float flashOutDuration = 1.0f;
    public Color flashColor = new Color(1f, 0.9f, 0.7f, 1f); // 태양빛 같은 뜨거운 색

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (distortionVolume != null)
        {
            distortionVolume.weight = 0f;
        }
    }

    public void TriggerHitStop()
    {
        StopAllCoroutines();
        StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1.0f;
    }

    // ★ 충돌 지점(impactPos)을 파라미터로 받습니다!
    public void TriggerShake(float intensityMultiplier, Vector3 impactPos)
    {
        // 1. 화면 흔들림
        if (targetToShake != null)
        {
            targetToShake.DOComplete();
            targetToShake.DOShakeAnchorPos(shakeDuration, shakeStrength * intensityMultiplier, shakeVibrato)
                         .SetUpdate(true);

            
        }

        // 2. 폭발적인 공간 왜곡 (중심점 이동)
        if (distortionVolume != null && distortionVolume.profile != null)
        {
            DOTween.Kill(distortionVolume);

            // ★ URP 볼륨에서 Lens Distortion 효과를 찾아서 중심점을 충돌 지점으로 변경합니다.
            if (distortionVolume.profile.TryGet<LensDistortion>(out var lensDistortion))
            {
                if (Camera.main != null)
                {
                    // 게임 월드 좌표를 화면 비율(0~1) 좌표로 변환하여 왜곡 중심(Center)에 넣습니다.
                    Vector2 viewportPos = Camera.main.WorldToViewportPoint(impactPos);
                    lensDistortion.center.value = viewportPos;
                }
            }

            // 가중치를 0.7 ~ 1.0 사이로 매우 강하게 터뜨림!
            float targetWeight = Mathf.Clamp01(0.7f + (intensityMultiplier * 0.1f));
            distortionVolume.weight = targetWeight;

            // 스르륵 원상복구
            DOTween.To(() => distortionVolume.weight, x => distortionVolume.weight = x, 0f, distortionDuration)
                   .SetTarget(distortionVolume)
                   .SetUpdate(true)
                   .SetEase(Ease.OutCirc); // OutCirc를 쓰면 초반에 강하게 터지고 부드럽게 가라앉습니다.
        }
    }

    public void TriggerGameOverFeel()
    {
        SoundManager.Instance.PlayGameOverSound();

        // 1. 일반 타격보다 훨씬 긴 '처절한' 역경직 (0.2초)
        StopAllCoroutines();
        StartCoroutine(GameOverHitStopRoutine());

        // 2. 화면 미친듯이 흔들기 (가중치 2배)
        TriggerShake(2f, Camera.main != null ? Camera.main.transform.position : Vector3.zero);

        // 3. 눈뽕(플래시) 연출
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f); // 투명에서 시작

            // SetUpdate(true)를 넣어야 역경직(시간정지) 중에도 번쩍입니다!
            flashOverlay.DOFade(flashColor.a, flashInDuration).SetUpdate(true)
                        .OnComplete(() =>
                        {
                            flashOverlay.DOFade(0f, flashOutDuration).SetUpdate(true)
                                        .OnComplete(() => flashOverlay.gameObject.SetActive(false));
                        });
        }
    }

    private IEnumerator GameOverHitStopRoutine()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = 1.0f;
    }
}