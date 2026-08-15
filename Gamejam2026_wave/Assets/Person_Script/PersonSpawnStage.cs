using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class PersonSpawnStage
{
    [Min(0)]
    public float startTime;

    [Header("Spawn Interval")]
    [Min(0.1f)]
    public float minSpawnInterval = 1f;

    [Min(0.1f)]
    public float maxSpawnInterval = 3f;

    // ★ 유저님 아이디어 추가! 필드 위 사람 수 제어
    [Header("Population Limits")]
    [Tooltip("이 숫자보다 사람이 적으면 강제로 빠르게 스폰시킵니다.")]
    [Min(0)]
    public int minActivePersons = 1;

    [Tooltip("이 숫자만큼 사람이 필드에 있으면 더 이상 스폰하지 않습니다.")]
    [Min(1)]
    public int maxActivePersons = 5;

    public List<PersonSpawnData> persons = new();
}