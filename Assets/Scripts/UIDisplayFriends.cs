using System.Collections;
using System.Collections.Generic;
using KnoxGameStudios;
using Photon.Realtime;
using UnityEngine;

public class UIDisplayFriends : MonoBehaviour
{
    [SerializeField] private Transform friendcontainer;
    [SerializeField] private UIFriend uiFriendPrefab;

    private void Awake()
    {
        PhotonFriendController.OnDisplayFriends += HandleDisplayFriends;
    }
    private void OnDestroy()
    {
        PhotonFriendController.OnDisplayFriends -= HandleDisplayFriends;
    }


    private void HandleDisplayFriends(List<FriendInfo> friends)
    {
        foreach (Transform child in friendcontainer)
        {
            Destroy(child.gameObject);
        }
        Debug.Log($"UI instantiate friends display {friends.Count}");
        foreach (FriendInfo friend in friends)
        {
            UIFriend uifriend = Instantiate(uiFriendPrefab, friendcontainer);
            uifriend.Initialize(friend);
        }
    }
}