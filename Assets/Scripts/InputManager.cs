using System;
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
   [SerializeField]
   private Camera sceneCamera;
   private Vector3 lastPosition;
   private Vector2 touchPos;
   [SerializeField]
   private LayerMask placementLayerMask;
   public event Action OnClicked;

    private PlayerInput playerInput;
    private InputAction touchPositionAction;
    private InputAction touchPressAction;
    public EventSystem eventSystem;
    public GraphicRaycaster uiRaycaster;
    private void Start() {
        if (eventSystem == null) {
            eventSystem = FindObjectOfType<EventSystem>();
        }
        if (uiRaycaster == null) {
            uiRaycaster = FindObjectOfType<GraphicRaycaster>();
        }
    }

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        touchPressAction = playerInput.actions["TouchPress"];
        touchPositionAction = playerInput.actions["TouchPosition"];
    }

    private void Update() {
        if (touchPressAction.WasPerformedThisFrame()) {
            // Debug.Log("pressed!");
            touchPos = touchPositionAction.ReadValue<Vector2>();
            if (!IsPointerOverUIObject(touchPos)) {
                OnClicked?.Invoke();
            }
        }
    }

    private bool IsPointerOverUIObject(Vector2 touchPosition)
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = touchPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(eventData, results);

        return results.Count > 0;
    }

   public Vector3 GetSelectedMapPosition() {
        Vector3 pos = new Vector3(touchPos.x, touchPos.y,  sceneCamera.nearClipPlane);
        Ray ray = sceneCamera.ScreenPointToRay(pos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, placementLayerMask)) {
            lastPosition = hit.point;
        }
        return lastPosition;
   }

    public GameObject GetTouchedGameObject() {
        Vector3 pos = new Vector3(touchPos.x, touchPos.y, sceneCamera.nearClipPlane);
        Ray ray = sceneCamera.ScreenPointToRay(pos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100)) {
            GameObject touchedObject = hit.collider.gameObject;
            Debug.Log("Touched GameObject: " + touchedObject.name);
            return touchedObject;
        }
        return null;
    }
}
