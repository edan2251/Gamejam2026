using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 방향 열거형
public enum SpawnDirection { Left, Right, Top, Bottom }

public class PersonSpawner : MonoBehaviour
{
    [Header("Person")]
    [SerializeField] private PersonSpawnManager spawnManager;

    [Header("UI Spawn Settings")]
    [Tooltip("사람들이 생성될 부모 캔버스 패널")]
    [SerializeField] private RectTransform personContainer;
    [SerializeField] private RectTransform spawnPoint; // 스폰 기준점이 될 UI 객체
    [SerializeField] private float spawnRange = 100f;  // 픽셀 단위 범위

    [Header("Targets (UI RectTransforms)")]
    [SerializeField] private List<RectTransform> targets;

    [Header("Direction")]
    [SerializeField] private SpawnDirection spawnDirection;

    private Coroutine spawnCoroutine;

    // 오브젝트가 켜질 때마다 코루틴 재시작!
    private void OnEnable()
    {
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    // 오브젝트가 꺼질 때 코루틴 안전하게 정지!
    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float spawnTime = spawnManager.GetRandomSpawnInterval();
            yield return new WaitForSeconds(spawnTime);
            SpawnPerson();
        }
    }

    public void SpawnPerson()
    {
        Vector2 spawnPosition = spawnPoint.anchoredPosition;

        switch (spawnDirection)
        {
            case SpawnDirection.Left:
            case SpawnDirection.Right:
                spawnPosition.y += Random.Range(-spawnRange, spawnRange);
                break;

            case SpawnDirection.Top:
            case SpawnDirection.Bottom:
                spawnPosition.x += Random.Range(-spawnRange, spawnRange);
                break;
        }

        Person personPrefab = spawnManager.GetRandomPerson();
        if (personPrefab == null) return;

        // UI 캔버스 자식으로 생성
        Person person = Instantiate(personPrefab, personContainer);
        person.rectTransform.anchoredPosition = spawnPosition;
        person.rectTransform.localScale = Vector3.one;

        // 타겟 랜덤 선택 후 전달 (Dot 생성용 부모 캔버스도 같이 넘김)
        RectTransform randomTarget = targets[Random.Range(0, targets.Count)];
        person.SetTarget(randomTarget, personContainer);
    }
}