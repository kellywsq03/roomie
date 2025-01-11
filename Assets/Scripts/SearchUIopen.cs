using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SearchUIopen : MonoBehaviour
{
    public GameObject ui;
    public TMP_Text message;

    // Start is called before the first frame update
    public void openSearch()
    {
        ui.SetActive(true);
    }


    // Update is called once per frame
    public void closeSearch()
    {
        message.text = " ";
        ui.SetActive(false);

    }
}