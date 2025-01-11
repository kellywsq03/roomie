using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[SerializeField]
public class PlayfabManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text messageText;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public GameObject transition;
    public TMP_InputField username;

    private const int MaxRetryAttempts = 10;


    public void RegisterButton() {
    if (passwordInput.text.Length < 6) {
        messageText.text = "Password too short!";
        return;
        }
    if (string.IsNullOrEmpty(username.text))
        {
        messageText.text = "Username is required!";
        return;
        }

        var request = new RegisterPlayFabUserRequest {
        Email = emailInput.text,
        Password = passwordInput.text,
        Username = username.text,
        RequireBothUsernameAndEmail = true
        };
    PlayFabProfileModels.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);

    }

    private void SetUsername(string name)
    {
        PlayerPrefs.SetString("USERNAME", name);
    
    }

    public void LoginButton() {
        var request = new LoginWithEmailAddressRequest {
            Email = emailInput.text,
            Password = passwordInput.text
        };
        PlayFabProfileModels.LoginWithEmailAddress(request, OnLoginSuccess,OnError);

    }
    public void ResetPasswordButton() {
        var request = new SendAccountRecoveryEmailRequest {
            Email= emailInput.text,
            TitleId = "62C44"
        };
        PlayFabProfileModels.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result) {
        SetUsername(username.text);
        RetrySaveUsernameToUserData(username.text, 0);
        RetryUpdateDisplayName(username.text, 0);


        transition.SetActive(true);
        Invoke("changeScene",3);
        messageText.text = "Registered and logged in!";
    }

    private void RetrySaveUsernameToUserData(string username, int attempt)
    {
        if (attempt >= MaxRetryAttempts)
        {
            messageText.text = "Failed to save username after several attempts.";
            return;
        }

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
                { "Username", username }
            },
            Permission = UserDataPermission.Public
        };
        PlayFabProfileModels.UpdateUserData(request,
            result => OnDataUpdateSuccess(result),
            error => {
                Debug.LogError($"SaveUsernameToUserData failed on attempt {attempt + 1}: {error.GenerateErrorReport()}");
                RetrySaveUsernameToUserData(username, attempt + 1); // Retry
            });
    }
    private void RetryUpdateDisplayName(string displayName, int attempt)
    {
        if (attempt >= MaxRetryAttempts)
        {
            messageText.text = "Failed to update display name after several attempts.";
            return;
        }

        Debug.Log($"Updating Playfab account's Display name to: {displayName}");
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = displayName };
        PlayFabProfileModels.UpdateUserTitleDisplayName(request,
            result => OnDisplayNameSuccess(result),
            error => {
                Debug.LogError($"UpdateDisplayName failed on attempt {attempt + 1}: {error.GenerateErrorReport()}");
                RetryUpdateDisplayName(displayName, attempt + 1); // Retry
            });
    }

    private void OnDataUpdateSuccess(UpdateUserDataResult result)
    {
        Debug.Log("User data updated successfully.");
    }
    void OnLoginSuccess(LoginResult result)
    {
        // Fetch user data to verify the username
        PlayFabProfileModels.GetUserData(new GetUserDataRequest(), OnGetUserDataSuccess, OnError);
    }

    private void OnGetUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("Username"))
        {
            string storedUsername = result.Data["Username"].Value;

            if (storedUsername == username.text)
            {
                transition.SetActive(true);
                Invoke("changeScene", 3);
                messageText.text = "Logged in!";
                Debug.Log("Successful login/account create!");
                SetUsername(username.text);
            }
            else
            {
                messageText.text = "Username does not match!";
                Debug.Log("Failed login: Username does not match.");
            }
        }
        else
        {
            messageText.text = "Username not found!";
            Debug.Log("Failed login: Username not found.");
        }
    }
    void OnError(PlayFabError error) {
        messageText.text = error.ErrorMessage;
        Debug.Log(error.GenerateErrorReport());
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result) {
        messageText.text = "Password reset mail sent!";
    }

    void changeScene() {
        SceneManager.LoadScene(1);
    }



    private void OnDisplayNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log($"You have updated the displayname of the playfab account!");
    }

}
