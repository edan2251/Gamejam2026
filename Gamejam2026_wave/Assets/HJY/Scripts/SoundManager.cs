using UnityEngine;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource; // 배경음 전용
    public AudioSource sfxSource; // 효과음 전용 (하나로 돌려쓰기)

    [Header("Audio Clips")]
    public AudioClip bgmClip;
    public AudioClip waveLaunchClip;
    public AudioClip hitImpactClip;
    public AudioClip scoreChimeClip;
    public AudioClip gameOverSizzleClip; // 타들어가는 소리

    [Header("BGM Ducking & Fade Settings")]
    public float fadeDuration = 2.0f;     // 시작 시 서서히 커지는 시간
    public float duckingVolume = 0.3f;    // 게임 오버 시 내려갈 BGM 음량
    private float originalVolume = 1.0f;  // 원래 BGM 음량 저장용

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (bgmSource != null)
        {
            originalVolume = bgmSource.volume; // 유저가 인스펙터에서 설정한 기본 볼륨 기억
        }

        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0f; // 처음에 0에서 시작
            bgmSource.Play();

            // 시작할 때 부드럽게 페이드 인!
            bgmSource.DOFade(originalVolume, fadeDuration).SetUpdate(true);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayGameOverSound()
    {
        sfxSource.Stop(); // 진행 중인 효과음 강제 정지
        PlaySFX(gameOverSizzleClip, 1.2f);
    }

    // ★ 게임 오버 시 BGM 음량을 살짝 낮춤 (Ducking)
    public void DuckBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.DOFade(duckingVolume, 0.5f).SetUpdate(true);
        }
    }

    // ★ 게임 오버 연출이 끝난 후 BGM 음량을 원래대로 복구
    public void RestoreBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.DOFade(originalVolume, 0.8f).SetUpdate(true);
        }
    }
}