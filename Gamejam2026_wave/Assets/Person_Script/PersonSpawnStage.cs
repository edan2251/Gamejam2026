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

    public List<PersonSpawnData> persons = new();



}
