using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayFabSavePlayerCoins : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text coinValue;
    public void Save() {
        var request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>()
            {
                new StatisticUpdate()
                {
                    StatisticName = "Coins",
                    Value = int.Parse(coinValue.text)
                }
            }
        };
        PlayFabProfileModels.UpdatePlayerStatistics(request, OnStatsUpdated, OnStatsUpdateError);
    
    }
    private void OnStatsUpdated(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Player coin value updated successfully.");
    }

    private void OnStatsUpdateError(PlayFabError error)
    {
        Debug.LogError("Failed to update player coins: " + error.ErrorMessage);
    }
}
