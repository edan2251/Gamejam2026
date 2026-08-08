using UnityEngine;
using System;

[Serializable]
public class PersonSpawnData
{
    public Person personPrefab;

    [Min(0)]
    public float spawnWeight;


}
