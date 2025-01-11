using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;
public class SellSystem : MonoBehaviour
{
    public List<GameObject> qtyList;
    [SerializeField] private GameObject noPlantError;
     public TMP_Text CoinHeader;
    private int[] currQty = new int[]{0, 0, 0, 0, 0, 0};
    [SerializeField] private PlantsDatabaseSO plantsData;
    public void InitialiseQty() {
        currQty = new int[]{0, 0, 0, 0, 0, 0};
    }

    public void UpdateQty(int plantID) {
        currQty[plantID]++;
        qtyList[plantID].GetComponent<TMP_Text>().text = currQty[plantID].ToString();
    }
    public void HarvestPlant(int plantID) {
        if (currQty[plantID] - 1 < 0) {
            StartCoroutine(ShowUIDelayed());
        } else {
            int amt = plantsData.objectsData[plantID].sellPrice;
            CoinHeader.text = $"{int.Parse(CoinHeader.text) + amt}";
            currQty[plantID]--;
            qtyList[plantID].GetComponent<TMP_Text>().text = currQty[plantID].ToString();
        }
    }

    private IEnumerator ShowUIDelayed() {
        noPlantError.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        noPlantError.SetActive(false);
   }
}
