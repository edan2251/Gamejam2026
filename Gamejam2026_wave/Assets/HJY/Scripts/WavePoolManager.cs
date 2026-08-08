using System.Collections.Generic;
using UnityEngine;

public class WavePoolManager : MonoBehaviour
{
    // 싱글톤(Singleton)으로 만들어 어디서든 쉽게 접근할 수 있게 합니다.
    public static WavePoolManager Instance { get; private set; }

    [Header("Wave Prefabs (UI)")]
    [Tooltip("0: 1칸, 1: 2칸, 2: 3칸 프리팹을 순서대로 넣어주세요")]
    public GameObject[] wavePrefabs;

    [Header("Pool Settings")]
    [Tooltip("게임 시작 시 미리 만들어둘 파도 개수")]
    public int initialPoolSize = 5;

    // 파도 크기(int)별로 대기열(Queue)을 관리하는 딕셔너리
    private Dictionary<int, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<int, Queue<GameObject>>();

        // 3종류의 파도 프리팹을 순회하며 풀을 생성합니다.
        for (int i = 0; i < wavePrefabs.Length; i++)
        {
            int waveSize = i + 1; // 크기는 1, 2, 3
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // 초기 개수만큼 미리 생성해서 큐에 넣어둡니다 (비활성화 상태)
            for (int j = 0; j < initialPoolSize; j++)
            {
                GameObject obj = Instantiate(wavePrefabs[i], transform); // 풀 매니저 자식으로 생성
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(waveSize, objectPool);
        }
    }

    // 파도를 꺼내는 함수
    public GameObject SpawnFromPool(int waveSize, Transform parent, Vector2 anchoredPos, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(waveSize)) return null;

        GameObject objectToSpawn;

        // 큐에 남은 파도가 있다면 꺼내고, 다 썼다면 새로 Instantiate 합니다 (안전 장치)
        if (poolDictionary[waveSize].Count > 0)
        {
            objectToSpawn = poolDictionary[waveSize].Dequeue();
        }
        else
        {
            objectToSpawn = Instantiate(wavePrefabs[waveSize - 1]);
        }

        // 활성화 및 위치/부모 갱신
        objectToSpawn.transform.SetParent(parent, false);
        objectToSpawn.SetActive(true);

        RectTransform rect = objectToSpawn.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.localRotation = rot;
        rect.localScale = Vector3.one;

        return objectToSpawn;
    }

    // 애니메이션이 끝나면 파도를 다시 풀(대기열)로 돌려보내는 함수
    public void ReturnToPool(GameObject obj, int waveSize)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform, false); // 화면에서 숨기고 부모 원상복구
        poolDictionary[waveSize].Enqueue(obj);
    }
}