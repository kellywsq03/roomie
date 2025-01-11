using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab.ClientModels;
using PlayFab;

public class PurchaseSystem : MonoBehaviour
{
    public List<GameObject> SelectButtons;
    
    public List<GameObject> PurchaseButtons;
    private List<int> purchasedItemIDs = new List<int>();
    public TMP_Text CoinHeader;
    public GameObject UPoor;
    public AudioSource purchaseSFX;
    
    void Start() {
      purchaseSFX = GetComponent<AudioSource>();
        GetPurchasedItems();
    }
   public void ActivateSelect(int ID) {
        // int amt = 0;
        int amt = int.Parse(PurchaseButtons[ID].GetComponentInChildren<TMP_Text>().text);
        int balance = int.Parse(CoinHeader.text) - amt;

        if (balance < 0) {
          StartCoroutine(ShowUIDelayed());
        } else {
          ChangeBalance(amt);
          SelectButtons[ID].SetActive(true);
          PurchaseButtons[ID].SetActive(false);
          purchaseSFX.Play();
            if (!purchasedItemIDs.Contains(ID))
            {
                purchasedItemIDs.Add(ID);
                UpdateButtons(ID);
                SavePurchasedItems();
            }

        }
   }
    private void UpdateButtons(int ID)
    {
        SelectButtons[ID].SetActive(true);
        PurchaseButtons[ID].SetActive(false);
    }

    // Save purchased item IDs to PlayFab
    private void SavePurchasedItems()
    {
        var data = new Dictionary<string, string>
        {
            { "PurchasedItems", string.Join(",", purchasedItemIDs) }
        };

        PlayFabProfileModels.UpdateUserData(new UpdateUserDataRequest
        {
            Data = data
        }, result =>
        {
            Debug.Log("Purchased items saved successfully.");
        }, error =>
        {
            Debug.LogError("Failed to save purchased items: " + error.GenerateErrorReport());
        });
    }

    // Get purchased items from PlayFab
    private void GetPurchasedItems()
    {
        PlayFabProfileModels.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PurchasedItems"))
            {
                string[] itemIDs = result.Data["PurchasedItems"].Value.Split(',');
                foreach (var id in itemIDs)
                {
                    if (int.TryParse(id, out int itemID))
                    {
                        purchasedItemIDs.Add(itemID);
                        UpdateButtons(itemID);
                    }
                }
            }
            Debug.Log("Purchased items loaded successfully.");
        }, error =>
        {
            Debug.LogError("Failed to get purchased items: " + error.GenerateErrorReport());
        });
    }
    private  IEnumerator ShowUIDelayed() {
     UPoor.SetActive(true);
     yield return new WaitForSeconds(0.5f);
     UPoor.SetActive(false);
   }
   public void ChangeBalance(int amt) {
        CoinHeader.text = $"{int.Parse(CoinHeader.text) - amt}";
   }
}
