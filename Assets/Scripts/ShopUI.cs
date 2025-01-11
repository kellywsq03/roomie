using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ShopUI : MonoBehaviour{

    public GameObject ui;

    // Start is called before the first frame update
    public void openShop() {
        ui.SetActive(true);
    }


    // Update is called once per frame
    public void closeShop() {
        ui.SetActive(false);
        
    }
}
