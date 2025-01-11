using System;
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;

// using System.Drawing;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using static UnityEditor.PlayerSettings;
using Newtonsoft.Json;
using System.Linq;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private GameObject mouseIndicator, cellIndicator;
    [SerializeField] private InputManager inputManager;

    [SerializeField] private Grid grid;
    [SerializeField]
    private ObjectsDatabaseSO database;
    private int selectedObjectIndex = -1;
    [SerializeField]
    private GameObject gridVisualisation, PlaceUI, ConfirmUI, StartDeleteUI, CancelDeleteUI, DeleteUI, avatar;
    [SerializeField]
    private PurchaseSystem purchaseSystem;
    private GameObject objectToPlace;
    private GridData furnitureData;
    private Renderer previewRenderer;
    private List<GameObject> placedGameObjects = new List<GameObject>();
    List<Vector3Int> cellPos = new List<Vector3Int>();
    private Dictionary<Vector3Int, GameObject> cellToObj = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<GameObject, List<Vector3Int>> objToCells = new Dictionary<GameObject, List<Vector3Int>>();
    private List<GameObject> cells = new List<GameObject>();
    private Material cellMat;
    private Vector3Int lastPos, lastAvatarPos;
    private bool isPlaced, isSelected;
    public AudioSource placementSFX;
    private GameObject selectedObject;
    private int currRotation = 0;

    private void Start()
    {
        placementSFX = GetComponent<AudioSource>();
        // Instantiates data for placed furniture

        furnitureData = new GridData();

        // Gets cell renderer and material
        objectToPlace = new GameObject();
        previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();
        cellMat = previewRenderer.material;
        lastPos = Vector3Int.zero;
        lastAvatarPos = Vector3Int.zero;
        FetchUserData();

    }

    public void CancelPlacement()
    {
        // Destroys furniture and cells
        if (!isPlaced)
        {
            Destroy(objectToPlace);
        }
        foreach (var c in cells)
        {
            Destroy(c);
        }

        // Dequeues PlaceStructure method
        inputManager.OnClicked -= PlaceStructure;

        // Sets UIs
        ConfirmUI.SetActive(false);
        selectedObjectIndex = -1;
        gridVisualisation.SetActive(false);
        cellIndicator.SetActive(false);
        avatar.SetActive(true);
    }

    public void StopPlacement()
    {
        // Checks if can place furniture there
        bool placementValidity = CheckPlacementValidity(lastPos, selectedObjectIndex);
        if (placementValidity)
        {
            // Updates furnitureData
            placedGameObjects.Add(objectToPlace);
            furnitureData.AddObjectAt(lastPos,
                                    database.objectsData[selectedObjectIndex].Size,
                                    database.objectsData[selectedObjectIndex].ID,
                                    placedGameObjects.Count - 1);

            isPlaced = true;
            Quaternion rotation = objectToPlace.transform.rotation;
            float angle = objectToPlace.transform.localRotation.eulerAngles.y;
            SaveObjectPlacement(objectToPlace.transform.position, angle, selectedObjectIndex);

            // Remembers cells which object is placed on
            objToCells[objectToPlace] = cellPos;
            for (int i = 0; i < cellPos.Count; i++)
            {
                cellToObj[cellPos[i]] = objectToPlace;
            }


        }
        else
        {
            CancelPlacement();
            return;
        }


        // Destroys cells
        foreach (var c in cells)
        {
            Destroy(c);
        }

        inputManager.OnClicked -= PlaceStructure;

        // Sets UIs
        ConfirmUI.SetActive(false);
        selectedObjectIndex = -1;
        gridVisualisation.SetActive(false);
        cellIndicator.SetActive(false);
        avatar.SetActive(true);
        
        // Places avatar at nearest empty cell
        HashSet<Vector3Int> emptyCells = furnitureData.emptyCells;
        avatar.transform.position = grid.CellToWorld(FindNearestVector(emptyCells, lastAvatarPos));
        avatar.transform.GetChild(0).gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 135, 0));
    }

    // Finds nearest empty cell to avatar's last position
    private Vector3Int FindNearestVector(HashSet<Vector3Int> emptyCells, Vector3Int lastPos)
    {
        Vector3Int nearestVector = lastPos;
        float minDistance = float.MaxValue;
        foreach (Vector3Int c in emptyCells)
        {
            float distance = Vector3Int.Distance(c, lastPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestVector = c;
            }
        }
        return nearestVector;
    }

    // Begins placement mode
    // Triggered by select button
    public void StartPlacement(int ID)
    {
        StopDeletion();
        isPlaced = false;
        // Deactivate avatar and remember last cell
        lastAvatarPos = grid.WorldToCell(avatar.transform.position);
        avatar.SetActive(false);

        // Creates new cell GameObjects
        cells = new();

        // Sets UIs
        PlaceUI.SetActive(false);
        ConfirmUI.SetActive(true);
        gridVisualisation.SetActive(true);

        // Creates furniture GameOject
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);

        Debug.Log(selectedObjectIndex);
        if (selectedObjectIndex < 0)
        {
            Debug.LogError($"No ID found {ID}");
        }
        objectToPlace = Instantiate(database.objectsData[selectedObjectIndex].Prefab0);
        objectToPlace.name = selectedObjectIndex.ToString();
        objectToPlace.SetActive(false);

        // Initialise cells to size of furnitrue
        Vector2Int size = database.objectsData[selectedObjectIndex].Size;
        for (int i = 0; i < size.x * size.y; i++)
        {
            GameObject newCell = Instantiate(cellIndicator);
            newCell.GetComponentInChildren<Renderer>().material = cellMat;
            newCell.SetActive(false);
            cells.Add(newCell);
        }

        // Queues PlaceStructure method when clicked
        inputManager.OnClicked += PlaceStructure;
    }

    // Method for placing the furniture on different cells
    private void PlaceStructure()
    {
        // Show object
        objectToPlace.SetActive(true);

        // Gets gridPosition
        Vector3 pos = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(pos);
        gridPosition.z = -1;
        lastPos = gridPosition;

        // Change cell  color
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewRenderer.sharedMaterial.color = placementValidity ? Color.green : Color.red;

        // Deactivates cells which are out of room bounds
        cellPos = furnitureData.CalculatePositions(gridPosition, database.objectsData[selectedObjectIndex].Size);
        for (int i = 0; i < cellPos.Count; i++)
        {
            if (!furnitureData.IsWithinBounds(cellPos[i]))
            {
                cells[i].SetActive(false);
            }
            else
            {
                cells[i].SetActive(true);
                cells[i].transform.position = grid.CellToWorld(cellPos[i]);
            }
        }

        // Places object at valid cell
        if (placementValidity)
        {
            objectToPlace.SetActive(true);
            objectToPlace.transform.position = grid.CellToWorld(gridPosition);
           
            placementSFX.Play();
        }
        else
        {
            objectToPlace.SetActive(false);
        }
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        return furnitureData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    public void StartDeletion()
    {
        CancelPlacement();
        Debug.Log("deletion started");
        DeleteUI.SetActive(true);
        CancelDeleteUI.SetActive(true);
        StartDeleteUI.SetActive(false);
        gridVisualisation.SetActive(true);
        // Creates new cell GameObjects
        cells = new();
        // Queues PlaceStructure method when clicked
        inputManager.OnClicked += SelectStructure;
    }

    public void StopDeletion()
    {
        isSelected = false;

        DeleteUI.SetActive(false);
        StartDeleteUI.SetActive(true);
        CancelDeleteUI.SetActive(false);

        gridVisualisation.SetActive(false);
        inputManager.OnClicked -= SelectStructure;
    }

    public void DeleteStructure()
    {
        if (!isSelected)
        {
            return;
        }
        List<Vector3Int> selectedCells = objToCells[selectedObject];
        int furnitureID = furnitureData.GetFurnitureID(selectedCells[0]);

        // Updates furniture data
        furnitureData.RemoveObjectAt(selectedCells);
        DeleteObjectData(selectedObject);

        foreach (var c in selectedCells)
        {
            cellToObj.Remove(c);
        }
        objToCells.Remove(selectedObject);
        for (int i = 0; i < cells.Count; i++)
        {
            Destroy(cells[i]);
        }

        placedGameObjects.Remove(selectedObject);

        cells.Clear();
        cellPos.Clear();



        Destroy(selectedObject);
        purchaseSystem.ActivateSelect(furnitureID);
        isSelected = false;
        

        if (placedGameObjects.Count < 1)
        {
            StopDeletion();
        }

    }
    private void SelectStructure()
    {
        if (placedGameObjects.Count == 0)
        {
            Debug.Log("cant find");
            return;
        }
        // Gets gridPosition
        Vector3 pos = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(pos);
        // selectedObject = inputManager.GetTouchedGameObject();
        gridPosition.z = -1;
        lastPos = gridPosition;
        Debug.Log(gridPosition);

        // Do nothing, no object selected
        if (!cellToObj.ContainsKey(gridPosition))
        {
            Debug.Log("no object");
            return;
        }

        isSelected = true;

        // Retrieves relevant GameObject
        selectedObject = cellToObj[gridPosition];

        // Change cell color
        previewRenderer.sharedMaterial.color = Color.red;

        // Finds cells which object is on
        List<Vector3Int> cellPos = objToCells[selectedObject];

        // Adds/Removes cells from list
        int cellsCount = cells.Count;
        if (cells.Count < cellPos.Count)
        {
            for (int i = 0; i < cellPos.Count - cellsCount; i++)
            {
                cells.Add(Instantiate(cellIndicator));
            }
        }
        else
        {
            for (int i = cellsCount; i > cellPos.Count; i--)
            {
                Destroy(cells[i - 1]);
                cells.RemoveAt(i - 1);
            }
        }

        // Moves cells to furniture location
        for (int i = 0; i < cellPos.Count; i++)
        {
            cells[i].SetActive(true);
            cells[i].transform.position = grid.CellToWorld(cellPos[i]);
        }
    }

    public void Rotate()
    {
        if (selectedObjectIndex < 0)
        {
            return;
        }
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == (selectedObjectIndex + 7) % 14);
        Debug.Log(selectedObjectIndex);
        Destroy(objectToPlace);
        currRotation = (currRotation + 1) % 4;
        Debug.Log(currRotation);
        if (currRotation == 0)
        {
            objectToPlace = Instantiate(database.objectsData[selectedObjectIndex].Prefab0);
        }
        else if (currRotation == 1)
        {
            objectToPlace = Instantiate(database.objectsData[selectedObjectIndex].Prefab0);
        }
        else if (currRotation == 2)
        {
            objectToPlace = Instantiate(database.objectsData[selectedObjectIndex].Prefab1);
        }
        else if (currRotation == 3)
        {
            objectToPlace = Instantiate(database.objectsData[selectedObjectIndex].Prefab1);
        }

        PlaceStructure();
    }
    private void SaveObjectPlacement(Vector3 position, float rotation, int objectId)
    {
        string key = objectId.ToString();
        string value = $"{position.x},{position.y},{position.z},{rotation}";

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { key, value }
        },
            Permission = UserDataPermission.Public
        };

        PlayFabProfileModels.UpdateUserData(request, OnDataSendSuccess, OnDataSendError);
    }

    private void OnDataSendSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data updated successfully.");
    }

    private void OnDataSendError(PlayFabError error)
    {
        Debug.LogError("Error updating data: " + error.GenerateErrorReport());
    }

    private void DeleteObjectData(GameObject obj)
    {
        string keyToDelete = obj.name;

        var request = new UpdateUserDataRequest
        {
            KeysToRemove = new List<string> { keyToDelete }
        };

        PlayFabProfileModels.UpdateUserData(request, OnDataDeleteSuccess, OnDataDeleteError);
    }

    private void OnDataDeleteSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data deleted successfully.");
    }

    private void OnDataDeleteError(PlayFabError error)
    {
        Debug.LogError("Error deleting data: " + error.GenerateErrorReport());
    }
    private void FetchUserData()
    {
        var request = new GetUserDataRequest();
        PlayFabProfileModels.GetUserData(request, OnGetUserDataSuccess, OnGetUserDataError);
    }

    private void OnGetUserDataSuccess(GetUserDataResult result)
    {
        Debug.Log("User data retrieved successfully.");
        if (result.Data != null && result.Data.Count > 0)
        {
            foreach (var keyValuePair in result.Data)
            {
                string key = keyValuePair.Key;
                string value = keyValuePair.Value.Value;

                // Parse the data
                string[] data = value.Split(',');
                if (data.Length == 4)
                {
                    int objectId;
                    if (int.TryParse(key, out objectId))
                    {
                        float x = float.Parse(data[0]);
                        float y = float.Parse(data[1]);
                        float z = float.Parse(data[2]);
                        float a = float.Parse(data[3]);


                        Debug.Log("yes");
                        // Spawn the object at the saved position
                        SpawnObjectAt(new Vector3(x, y, z), a, objectId);
                    }
                }
            }
        }
    }

    private void OnGetUserDataError(PlayFabError error)
    {
        Debug.LogError("Error retrieving user data: " + error.GenerateErrorReport());
    }
    public void SpawnObjectAt(Vector3 position, float b, int objectId)
    {
        Debug.Log("spawning..");
        // Find the object prefab from the database
        GameObject objectToPlace = Instantiate(database.objectsData[objectId].Prefab0, position, Quaternion.Euler(0, b, 0));
        objectToPlace.name = objectId.ToString();
        placedGameObjects.Add(objectToPlace);

    }


}