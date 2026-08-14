using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance { get; private set; }

    [Header("Hit Stop Settings (역경직)")]
    public float hitStopDuration = 0.05f;

    // ★ 카메라 쉐이크 -> UI 쉐이크로 변경!
    [Header("UI Shake Settings (화면 흔들림)")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 30f; // UI 픽셀 단위이므로 숫자를 좀 키웠습니다 (30~50 추천)
    public int shakeVibrato = 10;

    // 흔들고 싶은 대상 (Canvas 전체 또는 Grid 부모 패널)
    public RectTransform uiContainerToShake;

    [Header("Floating Text Settings (플로팅 텍스트)")]
    public GameObject floatingTextPrefab;
    public RectTransform floatingTextCanvas;

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

    // ★ 카메라 대신 연결된 UI(RectTransform)를 흔듭니다.
    public void TriggerCameraShake(float intensityMultiplier = 1f)
    {
        if (uiContainerToShake != null)
        {
            uiContainerToShake.DOComplete();
            // 2D UI에 맞춰 DOShakeAnchorPos 사용
            uiContainerToShake.DOShakeAnchorPos(shakeDuration, shakeStrength * intensityMultiplier, shakeVibrato);
        }
    }

    public void SpawnFloatingText(string textStr, Vector2 spawnPosition)
    {
        if (floatingTextPrefab == null || floatingTextCanvas == null) return;

        GameObject textObj = Instantiate(floatingTextPrefab, floatingTextCanvas);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();

        tmp.text = textStr;
        textRect.position = spawnPosition;

        textRect.DOAnchorPosY(textRect.anchoredPosition.y + 100f, 0.7f).SetEase(Ease.OutCirc);
        tmp.DOFade(0, 0.7f).OnComplete(() => Destroy(textObj));
    }
}