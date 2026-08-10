using UnityEngine;
using System.Collections.Generic;

public class PersonSpawnManager : MonoBehaviour
{
    [SerializeField] private List<PersonSpawnStage> stages = new();

    private float gameTime = 0f; // 명시적 초기화

    // 게임 시작 버튼을 누를 때 UIManager에서 호출하거나, OnEnable 시 초기화
    void OnEnable()
    {
        gameTime = 0f;
    }

    void Update()
    {
        gameTime += Time.deltaTime;
    }

    public Person GetRandomPerson()
    {
        PersonSpawnStage stage = GetCurrentStage();
        if (stage == null) return null;
        return GetRandomPersonFromStage(stage);
    }

    private PersonSpawnStage GetCurrentStage()
    {
        PersonSpawnStage currentStage = null;
        foreach (PersonSpawnStage stage in stages)
        {
            if (gameTime >= stage.startTime)
            {
                currentStage = stage;
            }
        }
        return currentStage;
    }

    private Person GetRandomPersonFromStage(PersonSpawnStage stage)
    {
        float totalWeight = 0f;
        foreach (PersonSpawnData data in stage.persons)
        {
            totalWeight += data.spawnWeight;
        }

        if (totalWeight <= 0f) return null;
        float randomValue = Random.Range(0f, totalWeight);

        foreach (PersonSpawnData data in stage.persons)
        {
            randomValue -= data.spawnWeight;
            if (randomValue <= 0f)
            {
                return data.personPrefab;
            }
        }
        return null;
    }

    public float GetRandomSpawnInterval()
    {
        PersonSpawnStage stage = GetCurrentStage();
        if (stage == null) return 3f;

        return Random.Range(stage.minSpawnInterval, stage.maxSpawnInterval);
    }
}