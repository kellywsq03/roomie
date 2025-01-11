using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


public class Avatar : MonoBehaviour

{   public AudioSource walkingSound;
    private bool isWalking;
    public int timer;
    [SerializeField] private float speed;
    private PlayerInput moveActionToUse;

    void Start() {
        walkingSound = GetComponent<AudioSource>();
        moveActionToUse = GetComponent<PlayerInput>();
        moveActionToUse.enabled = false;
        moveActionToUse.enabled = true;

    }

    void FixedUpdate()
    {

        Vector2 moveInput = moveActionToUse.actions["Move"].ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
        moveDir.Normalize();


        // Update isWalking for animation
        isWalking = moveDir != Vector3.zero;

        // Collider logic
        float moveDist = speed * Time.deltaTime;
        float avatarRadius = .3f;
        float avatarHeight = 100f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * avatarHeight, avatarRadius, moveDir, moveDist);
        
        if (canMove) {
            transform.position += moveDir * moveDist;
        }
        
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);

        if (isWalking && timer == 0) {
            walkingSound.Play();
            timer++;
        }
        if (!isWalking) {
            timer = 0;
            walkingSound.Stop();
        }
        

    }

    public bool IsWalking() {
        return isWalking;
    }


}
