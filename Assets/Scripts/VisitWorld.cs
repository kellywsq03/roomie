using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;


public class VisitWorld : MonoBehaviour
{
    public TMP_InputField inputFields;
    public TMP_Text message;
    public string username;
    public string playfabID;
    public static List<string[]> objectDataList = new List<string[]>();



    public void SetUserName()
    {
        if (inputFields != null)
        {
            username = inputFields.text;
            Debug.Log("Username set to: " + username);
        }
        else
        {
            Debug.LogError("Input field is not assigned.");
        }
    }

    public void LoadIfExists()
    {
        if (!string.IsNullOrEmpty(username))
        {

            var request = new GetAccountInfoRequest { Username = username };
            PlayFabProfileModels.GetAccountInfo(request, OnGetAccountInfoSuccess, OnGetAccountInfoFailure);
        }
        else
        {
            message.text = "Username cannot be empty.";
            Debug.LogError("Username is empty.");
        }
    }

    public void OnGetAccountInfoSuccess(GetAccountInfoResult result)
    {
        playfabID = result.AccountInfo.PlayFabId;
        Debug.Log("PlayFab ID retrieved: " + playfabID);

        var request = new GetUserDataRequest { PlayFabId = playfabID };
        PlayFabProfileModels.GetUserData(request, OnGetUserDataSuccess, OnGetUserDataError);
        
    }

    public void OnGetAccountInfoFailure(PlayFabError error)
    {
        message.text = "No such user!";
        Debug.LogError("Failed to get account info: " + error.GenerateErrorReport());
    }

    private void OnGetUserDataSuccess(GetUserDataResult result)
    {
        Debug.Log("User data retrieved successfully.");
        objectDataList.Clear(); // Clear previous data if any

        if (result.Data != null && result.Data.Count > 0)
        {
            foreach (var keyValuePair in result.Data)
            {
                string key = keyValuePair.Key;
                string value = keyValuePair.Value.Value;
                Debug.Log($"Key: {key}, Value: {value}");

                // Parse the data
                string[] data = value.Split(',');
                if (data.Length == 4)
                {
                    Debug.Log($"Parsed Data: {string.Join(", ", data)}");
                    objectDataList.Add(new string[] { key, data[0], data[1], data[2], data[3] });
                }
                else
                {
                    Debug.LogWarning($"Data format is incorrect for key: {key}, value: {value}");
                }
            }

            Debug.Log($"Total objects parsed: {objectDataList.Count}");
        }
        else
        {
            Debug.LogWarning("No user data found or data count is zero.");
        }

        // Load the next scene after processing data
        SceneManager.LoadScene(3);
    }

    private void OnGetUserDataError(PlayFabError error)
    {
        Debug.LogError("Error retrieving user data: " + error.GenerateErrorReport());
    }
}