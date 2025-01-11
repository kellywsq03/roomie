using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Photon.Realtime;

public class UIFriend : MonoBehaviour
{
    [SerializeField] private TMP_Text friendNameText;
    [SerializeField] private FriendInfo friend;

    public static Action<string> OnRemoveFriend = delegate { };

    public void Initialize(FriendInfo friend)
    {
        this.friend = friend;
        friendNameText.SetText(this.friend.UserId);
    }
    public void RemoveFriend()
    {
        OnRemoveFriend?.Invoke(friend.UserId);
        
    }
}
