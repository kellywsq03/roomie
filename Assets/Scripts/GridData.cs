using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;

[Serializable]
public class GridData
{
    public Dictionary<Vector3Int, PlacementData> placedObjects = new();
    public static int[] xBound = new int[] { -4, 2 };
    public static int[] yBound = new int[] { -3, 3 };
    public HashSet<Vector3Int> emptyCells = new HashSet<Vector3Int>();



    public int GetFurnitureID(Vector3Int gridPos)
    {
        return placedObjects[gridPos].ID;
    }


    public GridData()
    {
        // Initializes HashSet of empty cells
        for (int x = xBound[0]; x < xBound[1] + 1; x++)
        {
            for (int y = yBound[0]; y < yBound[1] + 1; y++)
            {
                emptyCells.Add(new Vector3Int(x, y, -1));
            }
        }
    }
    
    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                throw new Exception($"Dictionary already contains this cell position in {pos}");
            }
            placedObjects[pos] = data;
            emptyCells.Remove(pos);
        }
        
    }

    public void RemoveObjectAt(List<Vector3Int> gridPositions)
    {
        foreach (var pos in gridPositions)
        {
            placedObjects.Remove(pos);
            emptyCells.Add(pos);
        }
        
    }

    public List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, y, 0));
            }
        }
        
        return returnVal;
        
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                
                return false;
            }
            if (pos.x < xBound[0] || pos.x > xBound[1]
                || pos.y < yBound[0] || pos.y > yBound[1])
            {
                
                return false;
            }
        }
        
        return true;
        
    }

    public bool IsWithinBounds(Vector3Int pos)
    {
        if (pos.x < xBound[0] || pos.x > xBound[1]
            || pos.y < yBound[0] || pos.y > yBound[1])
        {
            Debug.Log("can't place!");
            
            return false;
        }
        
        return true;
    }
}

[Serializable]
public class PlacementData
{
    public List<Vector3Int> occupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}