using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimatorV3 : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Avatar avatar;
    private const string IS_WALKING = "IsWalking";

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        animator.SetBool(IS_WALKING, avatar.IsWalking());
    }
}
