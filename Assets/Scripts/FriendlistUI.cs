using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlistUI : MonoBehaviour
{
    public GameObject ui;

    
    public void Openfl()
    {
        ui.SetActive(true);
    }


    
    public void Closefl()
    {
        ui.SetActive(false);

    }
}
