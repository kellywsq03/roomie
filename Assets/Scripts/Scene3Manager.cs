using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene3Manager : MonoBehaviour
{
    [SerializeField]
    private ObjectsDatabaseSO database;
    void Start()
    {
        // Ensure that this method is called when the scene is loaded
        SpawnObjects();
    }


    private void SpawnObjects()
    {
        Debug.Log("visitworldspawn");
        foreach (var objectData in VisitWorld.objectDataList)
        {
            if (int.TryParse(objectData[0], out int objectId) &&
                float.TryParse(objectData[1], out float x) &&
                float.TryParse(objectData[2], out float y) &&
                float.TryParse(objectData[3], out float z) &&
                float.TryParse(objectData[4], out float a))
            {
                Debug.Log("Spawning object in Scene 3.");
                // Spawn the object at the saved position
                Vector3 position = new Vector3(x, y, z);
                GameObject objectToPlace = Instantiate(database.objectsData[objectId].Prefab0, position, Quaternion.Euler(0, a, 0));
                objectToPlace.name = objectId.ToString();
                
            }
            else
            {
                Debug.LogError("Error parsing object data.");
            }
        }
    }


}