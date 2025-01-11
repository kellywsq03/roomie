using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarUI : MonoBehaviour
{
    [SerializeField] private GameObject ui, openButton, closeButton;
    public void OpenToolbar()
    {
        openButton.SetActive(false);
        closeButton.SetActive(true);
        ui.SetActive(true);
    }

    public void CloseToolbar()
    {
        closeButton.SetActive(false);
        openButton.SetActive(true);
        ui.SetActive(false);
    }
}
