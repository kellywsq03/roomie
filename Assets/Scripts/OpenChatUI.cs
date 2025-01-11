using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChatUI : MonoBehaviour { 
    public GameObject ui;

 // Start is called before the first frame update
public void openChat()
{
    ui.SetActive(true);
}


// Update is called once per frame
public void closeChat()
{
    ui.SetActive(false);

}
}
