using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance { get; private set; }

    [Header("Hit Stop Settings (역경직)")]
    public float hitStopDuration = 0.05f;

    [Header("Shake Target (흔들 대상)")]
    // ★ 유저님 말씀대로 흔들 대상을 직접 끌어다 넣을 수 있게 만들었습니다!
    public RectTransform targetToShake;

    [Header("Shake Settings (흔들림 설정)")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 30f; // UI 기준이므로 30~50 정도의 큰 값을 넣어야 잘 보입니다.
    public int shakeVibrato = 10;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    // ★ 지정된 타겟(UI 패널)을 직접 흔듭니다!
    public void TriggerShake(float intensityMultiplier = 1f)
    {
        if (targetToShake != null)
        {
            targetToShake.DOComplete();
            // UI RectTransform 전용 흔들림 함수 (DOShakeAnchorPos)
            targetToShake.DOShakeAnchorPos(shakeDuration, shakeStrength * intensityMultiplier, shakeVibrato);
        }
        else
        {
            Debug.LogWarning("흔들 대상(Target To Shake)이 연결되지 않았습니다!");
        }
    }
}