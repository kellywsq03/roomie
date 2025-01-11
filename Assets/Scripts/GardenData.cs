using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GardenData 
{
   Dictionary<Vector3Int, PlotData> placedObjects = new();
   private static int[] xBound = new int[] {-4, 2};
   private static int[] yBound = new int[] {-3, 3};
   private GameObject[,] soilPlots;
   public HashSet<Vector3Int> emptyCells = new HashSet<Vector3Int>();
   Dictionary<GameObject, SoilData> soilData = new();
   private float testTime = 2f; // 2 seconds
   public GardenPlacementSystem gardenPlacementSystem;
   public int numMaturePlants = 0;
   public GardenData() {
        // Initializes HashSet of empty cells
        for (int x = xBound[0]; x < xBound[1] + 1; x++) {
            for (int y = yBound[0]; y < yBound[1] + 1; y++) {
                emptyCells.Add(new Vector3Int(x, y, -1));
            }
        }

        // Initialises 2d array of soil
        soilPlots = new GameObject[xBound[1] - xBound[0] + 1, yBound[1] - yBound[0] + 1];
   }
   public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex) {
        if (placedObjects.ContainsKey(gridPosition)) {
            throw new Exception($"Dictionary already contains this cell position in {gridPosition}");
        }
        emptyCells.Remove(gridPosition);
   }

   public List<Vector3Int> FindMatureSoil()
    {
        List<Vector3Int> matureSoils = new List<Vector3Int>();
        foreach (KeyValuePair<GameObject, SoilData> s in soilData) {
            SoilData currSoilData = s.Value;
            if (currSoilData.stage == 3) {
                matureSoils.Add(currSoilData.position);
            }
        }
        return matureSoils;
    }
   public void AddSoil(GameObject soil, int x, int y) {
        soilData[soil] = new SoilData(2f, new Vector3Int(x, y, -1));
        soilPlots[x - xBound[0], y - yBound[0]] = soil;
   }

   public GameObject GetSoil(int x, int y) {
        return soilPlots[x - xBound[0], y - yBound[0]];
   }
   public int GetPlantID(GameObject soil) {
    return soilData[soil].plantID;
   }

   public bool IsSoilWet(GameObject soil) {
        return soilData[soil].isWatered;
   }

   public int PlantStage(GameObject soil) {
        return soilData[soil].stage;
   }

   public void UpdateSoilTime(float time) {
        foreach (KeyValuePair<GameObject, SoilData> s in soilData) {
            SoilData currSoilData = s.Value;

            // For immature soil
            if (currSoilData.stage < 3) {
                if (currSoilData.isWatered) {
                currSoilData.time -= time;

                // Finished watering
                if (currSoilData.time < 0) {
                    currSoilData.isWatered = false;

                    // Changes render of soil to dry soil
                    gardenPlacementSystem.UpdateSoilType(s.Key);

                    // Increases plant stage
                    if (currSoilData.isPlanted) {
                        currSoilData.stage++;
                        currSoilData.plant = gardenPlacementSystem.UpdatePlantType(currSoilData.plantID, currSoilData.stage, currSoilData.plant, currSoilData.position);
                        // AddObjectAt(currSoilData.position, new Vector2Int(1, 1), 0, currSoilData.plantID);
                        if (currSoilData.stage == 3) {
                            numMaturePlants++;
                        }
                    }
                }
                }
            }
            
        }
   }

   public void PlantSoil(GameObject soil, int ID) {
        soilData[soil].isPlanted = true;
        soilData[soil].stage++;
        soilData[soil].plantID = ID;
   }
   
    public void WaterSoil(GameObject soil) {
        soilData[soil].time = testTime;
        soilData[soil].isWatered = true;
    }

    public GameObject HarvestPlant(GameObject soil) {
        soilData[soil].isPlanted = false;
        soilData[soil].time = 0;
        soilData[soil].stage = -1;
        soilData[soil].plantID = -1;
        numMaturePlants -= 1;
        return soilData[soil].plant;
    }

    public List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();
        for (int x = 0; x < objectSize.x; x++) {
            for (int y = 0; y < objectSize.y; y++) {
                returnVal.Add(gridPosition + new Vector3Int(x, y, 0));
            }
        }
        return returnVal;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize) {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach (var pos in positionToOccupy) {
            if (placedObjects.ContainsKey(pos)) {
                return false;
            }
            if (pos.x < xBound[0] || pos.x > xBound[1]
                || pos.y < yBound[0] || pos.y > yBound[1]) {
                    return false;
            }
        }
        return true;
    }

    public bool IsWithinBounds(Vector3Int pos) {
        if (pos.x < xBound[0] || pos.x > xBound[1]
                || pos.y < yBound[0] || pos.y > yBound[1]) {
                    Debug.Log("can't place!");
                    return false;
        }
        return true;
    }
}

// Data for taken plots
public class PlotData
{
    public List<Vector3Int> occupiedPositions;
    public int ID {get; private set;}
    public int PlacedObjectIndex {get; private set;}
    public PlotData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex) {
        this.occupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}

// Data for condition of soil & plant
public class SoilData
{
    // public GameObject soil {get; private set;}
    public float time {get; set;}
    public bool isWatered = false;
    public bool isPlanted = false;
    public int stage = -1;
    public GameObject plant;
    public int plantID;
    public Vector3Int position;
    public SoilData(float time, Vector3Int position) {
        this.time = time;
        this.position = position;
    }
}