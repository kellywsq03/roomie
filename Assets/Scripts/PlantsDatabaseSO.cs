using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlantsDatabaseSO : ScriptableObject
{
    public List<PlantData> objectsData;
}
[Serializable]
public class PlantData {
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public GameObject Stage1 { get; private set; }
    // public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField]
    public GameObject Stage2 { get; private set; }
    [field: SerializeField]
    public GameObject Stage3 { get; private set; }
    [field: SerializeField]
    public int sellPrice { get; private set; }
}