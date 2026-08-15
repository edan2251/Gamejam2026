using UnityEngine;
using System.Collections.Generic;

public class PersonSpawnManager : MonoBehaviour
{
    [SerializeField] private List<PersonSpawnStage> stages = new();
    private float gameTime = 0f;

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

    // ★ 추가 1: 현재 스폰이 가능한 상태인지(최대 인원 초과 안 했는지) 확인
    public bool CanSpawn()
    {
        PersonSpawnStage stage = GetCurrentStage();
        if (stage == null) return true;

        // 현재 필드에 있는 사람이 최대 허용치보다 적을 때만 true 반환
        return Person.ActivePersons.Count < stage.maxActivePersons;
    }

    // ★ 추가 2: 최소 인원 미만이면 빠르게 스폰하도록 인터벌 조정
    public float GetRandomSpawnInterval()
    {
        PersonSpawnStage stage = GetCurrentStage();
        if (stage == null) return 3f;

        // 필드에 사람이 너무 없다면(최소 인원 미만), 정해진 쿨타임을 무시하고 0.5초 만에 긴급 스폰!
        if (Person.ActivePersons.Count < stage.minActivePersons)
        {
            return 0.5f;
        }

        return Random.Range(stage.minSpawnInterval, stage.maxSpawnInterval);
    }
}