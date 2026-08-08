using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ManaManager : MonoBehaviour
{
    [Header("Mana Settings")]
    public int maxMana = 6;
    public float currentMana = 6f;

    [Header("Regen Settings")]
    public float regenInterval = 2.0f;
    private float regenTimer = 0f;

    [Header("UI References")]
    public RectTransform waveRect;

    [Header("Position Settings")]
    public float introManaPosY = 260f;
    public float maxManaPosY = -70f;
    public float zeroManaPosY = -2340f;

    [Header("Animation Settings")]
    public float waveTransitionDuration = 0.8f;
    [Tooltip("살랑거리는 속도")]
    public float swaySpeed = 2f;
    [Tooltip("살랑거리는 높낮이")]
    public float swayHeight = 20f;

    private float currentBaseY;
    private Tweener moveTweener;

    void Start()
    {
        currentBaseY = introManaPosY;
    }

    public void PlayIntroWaveAnimation()
    {
        moveTweener?.Kill();
        moveTweener = DOTween.To(() => currentBaseY, x => currentBaseY = x, maxManaPosY, 0.85f)
                             .SetEase(Ease.InOutCubic);
    }

    void Update()
    {
        float swayOffset = Mathf.Sin(Time.time * swaySpeed) * swayHeight;
        waveRect.anchoredPosition = new Vector2(waveRect.anchoredPosition.x, currentBaseY + swayOffset);

        //// Test = 스페이스바 
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    UseMana();
        //}

        HandleManaRegen();
    }

    private void HandleManaRegen()
    {
        if (currentMana < maxMana)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= regenInterval)
            {
                regenTimer = 0f;
                currentMana += 1f;

                if (currentMana > maxMana)
                {
                    currentMana = maxMana;
                }
                UpdateWavePosition();
            }
        }
        else
        {
            regenTimer = 0f;
        }
    }

    public void UseMana(int amount = 1)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateWavePosition();
        }
    }

    private void UpdateWavePosition()
    {
        float manaRatio = currentMana / maxMana;
        float targetPosY = Mathf.Lerp(zeroManaPosY, maxManaPosY, manaRatio);

        moveTweener?.Kill();
        moveTweener = DOTween.To(() => currentBaseY, x => currentBaseY = x, targetPosY, waveTransitionDuration)
                             .SetEase(Ease.OutCubic);

        if (currentMana <= 0)
        {
            //마나 없을때 파도 생성 막기
        }
    }
}