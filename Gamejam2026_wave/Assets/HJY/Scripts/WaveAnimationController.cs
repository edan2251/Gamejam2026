using UnityEngine;

public class WaveAnimationController : MonoBehaviour
{
    [Tooltip("이 파도의 크기를 적어주세요 (1, 2, 3)")]
    public int myWaveSize;

    public void WaveEnd()
    {
        // 1. 자식 히트박스에 붙어있는 WaveFront를 찾아 점수 정산 및 사람 제거 실행!
        WaveFront waveFront = GetComponentInChildren<WaveFront>();
        if (waveFront != null)
        {
            waveFront.ProcessWaveEndScoreAndCleanup();
        }
    }

    // 애니메이션의 제일 마지막 프레임(Animation Event)에서 이 함수를 호출합니다!
    public void OnAnimationFinished()
    {
        

        // 2. 기존대로 파도 오브젝트를 풀(Pool)로 반환
        WavePoolManager.Instance.ReturnToPool(gameObject, myWaveSize);
    }
}