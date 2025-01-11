using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayFabLoad : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text coinValue;

    public void GetPlayerStatistics()
    {
        var request = new GetPlayerStatisticsRequest();
        PlayFabProfileModels.GetPlayerStatistics(request, OnStatsRetrieved, OnStatsRetrieveError);
    }

    private void OnStatsRetrieved(GetPlayerStatisticsResult result)
    {
        foreach (var statistics in result.Statistics) {
            Debug.Log("StatisticsName : " + statistics.StatisticName + " Value: " + statistics.Value);
            coinValue.text = $"{statistics.Value}";
        }
    
    }
    private void OnStatsRetrieveError(PlayFabError error) 
    {
        Debug.LogError("Failed to retrieve player statistics: " + error.ErrorMessage);
    }
}
