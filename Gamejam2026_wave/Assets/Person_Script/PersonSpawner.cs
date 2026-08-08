using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static PersonSpawner;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

public enum SpawnDirection
{
    Left,
    Right,
    Top,
    Bottom
}


public class PersonSpawner : MonoBehaviour
{
    [Header("Person")]
    [SerializeField] private Person personPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRange = 5f;

    [Header("Targets")]
    [SerializeField] private List<Transform> targets;

    [Header("Direction")]
    [SerializeField] private SpawnDirection spawnDirection;

    [Header("Spawn Time")]
    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float maxSpawnTime = 3f;



    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float spawnTime = Random.Range(
                minSpawnTime,
                maxSpawnTime
            );

            yield return new WaitForSeconds(spawnTime);

            SpawnPerson();
        }
    }


    public void SpawnPerson()
    {
        Vector2 spawnPosition = spawnPoint.position;

        switch (spawnDirection)
        {
            case SpawnDirection.Left:
            case SpawnDirection.Right:
                // 좌우 스포너 → Y가 랜덤
                spawnPosition.y += Random.Range(
                    -spawnRange,
                    spawnRange
                );
                break;

            case SpawnDirection.Top:
            case SpawnDirection.Bottom:
                // 상하 스포너 → X가 랜덤
                spawnPosition.x += Random.Range(
                    -spawnRange,
                    spawnRange
                );
                break;
        }

        // 사람 생성
        Person person = Instantiate(
            personPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // 타겟 랜덤 선택
        Transform randomTarget = targets[
            Random.Range(0, targets.Count)
        ];

        // 사람에게 타겟 전달
        person.SetTarget(randomTarget);
    }
}
