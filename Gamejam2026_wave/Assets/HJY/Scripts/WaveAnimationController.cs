using UnityEngine;

public class WaveAnimationController : MonoBehaviour
{
    [Tooltip("이 파도의 크기를 적어주세요 (1, 2, 3)")]
    public int myWaveSize;

    // 애니메이션의 제일 마지막 프레임(Animation Event)에서 이 함수를 호출합니다!
    public void OnAnimationFinished()
    {
        WavePoolManager.Instance.ReturnToPool(gameObject, myWaveSize);
    }
}
