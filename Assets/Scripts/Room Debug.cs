using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider != null && meshCollider.sharedMesh == null)
        {
            Debug.LogError("MeshCollider has no mesh assigned.");
        } else {
            Debug.Log("Alright");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
