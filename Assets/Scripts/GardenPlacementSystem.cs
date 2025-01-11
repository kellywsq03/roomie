using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


// using System.Drawing;
using UnityEngine;

public class GardenPlacementSystem : MonoBehaviour
{
    [SerializeField] private GameObject soil, cellIndicator;
    [SerializeField] private Material wetSoil, drySoil, wetSoilSeeds, drySoilSeeds;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Grid grid;
    [SerializeField] private SellSystem sellSystem;
    [SerializeField]
    private PlantsDatabaseSO database;
    private int selectedObjectIndex = -1;
    [SerializeField]
    private GameObject gridVisualisation, PlaceUI, ConfirmUI, avatar, WaterUI, WaterDoneUI, HarvestErrorUI, StartHarvestUI, StopHarvestUI;
    private GameObject objectToPlace;
    private GardenData gardenData;
    private Renderer previewRenderer;
    private List<GameObject> placedGameObjects  = new List<GameObject>();
    private List<GameObject> cells = new List<GameObject>();
    private Material cellMat;
    private Vector3Int lastPos, lastAvatarPos;
    private List<GameObject> lastSoils = new List<GameObject>();
    private List<Vector3Int> lastSoilCoords = new List<Vector3Int>();
    private bool isPlaced;
    private static int[] xBound = new int[] {-4, 2};
    private static int[] yBound = new int[] {-3, 3};
    private Dictionary<Vector3Int, GameObject> gridCellInd;
    private bool canHarvest = false;

    private bool isWatering, isHarvesting;
    
    float timer;
    private void Start() {
        // Instantiates data for garden
        gardenData =  new GardenData();
        gardenData.gardenPlacementSystem = this;

        sellSystem.InitialiseQty();

        // Gets cell renderer and material
        objectToPlace = new GameObject();
        
        lastPos = Vector3Int.zero;
        lastAvatarPos = Vector3Int.zero;

        // Instantiates soil
        for (int x = xBound[0]; x < xBound[1] + 1; x++) {
            for (int y = yBound[0]; y < yBound[1] + 1; y++) {
                GameObject newSoil = Instantiate(soil);
                newSoil.SetActive(true);
                gardenData.AddSoil(newSoil, x, y);
                newSoil.transform.position = grid.CellToWorld(new Vector3Int(x, y, -1));
                newSoil.GetComponentInChildren<Renderer>().material = drySoil; 
            }
        }
    }

    public void CancelPlacement() {
        // Destroys furniture and cells
        foreach (var s in lastSoils) {
            if (gardenData.IsSoilWet(s)) {
                s.GetComponentInChildren<Renderer>().material = wetSoil;
            } else {
                s.GetComponentInChildren<Renderer>().material = drySoil;
            }
        }

        lastSoils.Clear();
        lastSoilCoords.Clear();

        // Dequeues PlaceStructure method
        inputManager.OnClicked -= PlaceStructure;
        
        // Sets UIs
        ConfirmUI.SetActive(false);
        selectedObjectIndex = -1;
        gridVisualisation.SetActive(false);
        avatar.SetActive(true);
    }

    public void StopPlacement()
    {
        foreach (var s in lastSoils) {
            gardenData.PlantSoil(s, selectedObjectIndex);
        }
        
        foreach (var s in lastSoilCoords) {
            gardenData.AddObjectAt(s, new Vector2Int(1, 1), 0, selectedObjectIndex);
        }

        lastSoils.Clear();
        lastSoilCoords.Clear();

        inputManager.OnClicked -= PlaceStructure;
        
        // Sets UIs
        ConfirmUI.SetActive(false);
        selectedObjectIndex = -1;
        gridVisualisation.SetActive(false);
        avatar.SetActive(true);

        // Places avatar at nearest empty cell
        HashSet<Vector3Int> emptyCells = gardenData.emptyCells;
        avatar.transform.position = grid.CellToWorld(FindNearestVector(emptyCells, lastAvatarPos));
        avatar.transform.GetChild(0).gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 135, 0));
    }

    // Finds nearest empty cell to avatar's last position
    private Vector3Int FindNearestVector(HashSet<Vector3Int> emptyCells, Vector3Int lastPos) {
        Vector3Int nearestVector = lastPos;
        float minDistance = float.MaxValue;
        foreach (Vector3Int c in emptyCells) {
            float distance = Vector3Int.Distance(c, lastPos);
            if (distance < minDistance) {
                minDistance = distance;
                nearestVector = c;
            }
        }
        return nearestVector;
    }

    // Begins placement mode
    // Triggered by select button
    public void StartPlacement(int ID) {
        CancelHarvesting();
        CancelWatering();
        lastSoils = new List<GameObject>();
        selectedObjectIndex = ID;

        // Deactivate avatar and remember last cell
        lastAvatarPos = grid.WorldToCell(avatar.transform.position);
        avatar.SetActive(false);
        
        // Sets UIs
        PlaceUI.SetActive(false);
        ConfirmUI.SetActive(true);
        gridVisualisation.SetActive(true);

        // Queues PlaceStructure method when clicked
        inputManager.OnClicked += PlaceStructure;
    }

    // Place seeds on different cells
    private void PlaceStructure()
    {
        // Gets gridPosition
        Vector3 pos = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition =  grid.WorldToCell(pos);
        gridPosition.z = -1;
        lastPos = gridPosition;
        objectToPlace = gardenData.GetSoil(gridPosition.x, gridPosition.y);

        // Places seed
        if (gardenData.IsSoilWet(objectToPlace)) {
            // Empty plot of soil
            if (gardenData.PlantStage(objectToPlace) < 0) {
                lastSoils.Add(objectToPlace);
                lastSoilCoords.Add(gridPosition);
            }
            objectToPlace.GetComponentInChildren<Renderer>().material = wetSoilSeeds;
        } else {
            if (gardenData.PlantStage(objectToPlace) < 0) {
                lastSoils.Add(objectToPlace);
                lastSoilCoords.Add(gridPosition);

            }
            objectToPlace.GetComponentInChildren<Renderer>().material = drySoilSeeds;
        }
    }

    public void StartWatering() {
        CancelPlacement();
        CancelHarvesting();
        lastAvatarPos = grid.WorldToCell(avatar.transform.position);
        avatar.SetActive(false);
        WaterDoneUI.SetActive(true);
        WaterUI.SetActive(false);
        gridVisualisation.SetActive(true);
        inputManager.OnClicked += WaterSoil;
        
        isWatering = true;
    }

    public void StopWatering() {
        isWatering = false;
        WaterDoneUI.SetActive(false);
        WaterUI.SetActive(true);
        inputManager.OnClicked -= WaterSoil;
        gridVisualisation.SetActive(false);
        
        HashSet<Vector3Int> emptyCells = gardenData.emptyCells;
        avatar.transform.position = grid.CellToWorld(FindNearestVector(emptyCells, lastAvatarPos));
        avatar.transform.GetChild(0).gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 135, 0));
        avatar.SetActive(true);
    }

    public void CancelWatering() {
        gridVisualisation.SetActive(false);
        if (isWatering) {
            StopWatering();
            isWatering = false;
        }
    }

    private void WaterSoil() {
        // Gets gridPosition
        Vector3 pos = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition =  grid.WorldToCell(pos);
        gridPosition.z = -1;
        lastPos = gridPosition;

        GameObject currSoil = gardenData.GetSoil(gridPosition.x, gridPosition.y);

        // Changes drySoil material to wetSoil material
        if (gardenData.PlantStage(currSoil) == 0) {
            currSoil.GetComponentInChildren<Renderer>().material = wetSoilSeeds;
        } else {
            currSoil.GetComponentInChildren<Renderer>().material = wetSoil;
        }

        // Waters dry soil
        if (!gardenData.IsSoilWet(currSoil)) {
            gardenData.WaterSoil(currSoil);
        }
    }

    public void UpdateSoilType(GameObject s) {
        s.GetComponentInChildren<Renderer>().material = drySoil;
    }

    public GameObject UpdatePlantType(int plantID, int stage, GameObject plant, Vector3Int position) {
        Destroy(plant);
        GameObject newPlant = new GameObject();
        if (stage == 1) {
            Destroy(newPlant);
            newPlant = Instantiate(database.objectsData[plantID].Stage1);
        }
        else if (stage == 2) {
            Destroy(newPlant);
            newPlant = Instantiate(database.objectsData[plantID].Stage2);
        } else if (stage == 3) {
            Destroy(newPlant);
            newPlant = Instantiate(database.objectsData[plantID].Stage3);
        }
        newPlant.transform.position = grid.CellToWorld(position);
        return newPlant;
    }

    // Updates soil watering time
    private void Update() {
        gardenData.UpdateSoilTime(Time.deltaTime);
        if (gardenData.numMaturePlants > 0) {
            canHarvest = true;
        } else {
            canHarvest = false;
        }
    }

    public void ShowMatureSoils() {
        gridCellInd = new Dictionary<Vector3Int, GameObject>();
        List<Vector3Int> matureSoils = gardenData.FindMatureSoil();
        foreach (var s in matureSoils) {
            GameObject newCell = Instantiate(cellIndicator);
            newCell.SetActive(true);
            cells.Add(newCell);
            newCell.transform.position = grid.CellToWorld(s);
            gridCellInd[s] = newCell;
        }
    }

    public void HideMatureSoils() {
        foreach(var c in cells) {
            c.SetActive(false);
        }
    }
    private IEnumerator ShowUIDelayed() {
        HarvestErrorUI.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        HarvestErrorUI.SetActive(false);
   }

    public void StartHarvesting() {
        CancelPlacement();
        CancelWatering();
        lastAvatarPos = grid.WorldToCell(avatar.transform.position);
        avatar.SetActive(false);
        StartHarvestUI.SetActive(false);
        StopHarvestUI.SetActive(true);
        if (!canHarvest) {
            StartCoroutine(ShowUIDelayed());
            isHarvesting = true;
            CancelHarvesting();
        } else {
            ShowMatureSoils();
            inputManager.OnClicked += HarvestPlant;
        }
        isHarvesting = true;
    }

    public void StopHarvesting() {
        HideMatureSoils();
        isHarvesting = false;
        StartHarvestUI.SetActive(true);
        StopHarvestUI.SetActive(false);
        inputManager.OnClicked -= HarvestPlant;
        HashSet<Vector3Int> emptyCells = gardenData.emptyCells;
        avatar.transform.position = grid.CellToWorld(FindNearestVector(emptyCells, lastAvatarPos));
        avatar.transform.GetChild(0).gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 135, 0));
        avatar.SetActive(true);
    }

    public void CancelHarvesting() {
        if (isHarvesting) {
            StopHarvesting();
            isHarvesting = false;
            avatar.SetActive(true);
        }
    }
    public void HarvestPlant() {
         // Gets gridPosition
        Vector3 pos = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition =  grid.WorldToCell(pos);
        gridPosition.z = -1;
        lastPos = gridPosition;

        List<Vector3Int> matureSoils = gardenData.FindMatureSoil();

        if (matureSoils.Contains(gridPosition)) {
            GameObject currSoil = gardenData.GetSoil(gridPosition.x, gridPosition.y);
            sellSystem.UpdateQty(gardenData.GetPlantID(currSoil));
            Destroy(gardenData.HarvestPlant(currSoil));
            cells.Remove(gridCellInd[gridPosition]);
            Destroy(gridCellInd[gridPosition]);
        }
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        return gardenData.CanPlaceObjectAt(gridPosition, new Vector2Int(1, 1));
    }

}