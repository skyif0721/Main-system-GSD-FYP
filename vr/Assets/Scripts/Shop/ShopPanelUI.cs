using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
// using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    public enum FacingTarget { Camera, Player, Hybrid }

    [Header("Components")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Button ShopExitButton;
    [SerializeField] private GameObject buyableMark;

    [Header("Behavior")]
    public Transform player;
    public Camera uiCamera;
    public FacingTarget faceTarget = FacingTarget.Camera;

    // [Header("Cinemachine")]
    // public CinemachineInputAxisController inputAxisController;

    // private @XRIDefaultInputActions inputActions;

    private bool playerIsNear = false;
    private bool inShop = false;

    Action targetAction;

    private void Awake()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (contentParent != null) 
            contentParent.SetActive(false);
        else
            Debug.LogWarning($"{name}: contentParent is not assigned.");
       
        if (!uiCamera) uiCamera = Camera.main;
        inShop = false;
        UpdateTargetAction();
    }

    private void Start()
    {
        if (uiCamera == null) uiCamera = Camera.main;
    }

    private void OnEnable()
    {
        ShopExitButton.onClick.AddListener(ExitShop);

        var gem = GameEventsManager.instance;
        if (gem?.inputEvents != null)
        {
            gem.inputEvents.onOpenShopPressed += OpenShopPressed;
        }
        else
        {
            Debug.LogWarning($"{name}: GameEventsManager.instance or inputEvents is null in OnEnable.");
        }

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            if (uiCamera == null) uiCamera = Camera.main;
            canvas.worldCamera = uiCamera;
        }
        else
        {
            Debug.LogError($"{name}: Canvas is missing. Assign it or add a Canvas component.");
        }

        if (ShopExitButton != null)
        {
            ShopExitButton.onClick.RemoveAllListeners();
            ShopExitButton.onClick.AddListener(() => ExitShop());
        }
        else
        {
            Debug.LogError($"{name}: ShopExitButton is not assigned.");
        }

    }

    private void OnDisable()
    {
        if (ShopExitButton != null)
            ShopExitButton.onClick.RemoveListener(ExitShop);

        var gem = GameEventsManager.instance;
        if (gem?.inputEvents != null)
            gem.inputEvents.onOpenShopPressed -= OpenShopPressed;
    }
    
    private void UpdateTargetAction()
    {
        targetAction = faceTarget switch
        {
            FacingTarget.Camera => FaceCamera,
            _ =>null
        };
    }

    private void FaceCamera()
    {
        if (!canvas || !uiCamera)
        {
            return;
        }
        var toCam = canvas.transform.position - uiCamera.transform.position;
        canvas.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }

    private void LateUpdate()
    {
        if (!canvas && canvas.gameObject.activeSelf) targetAction?.Invoke();

        if (!playerIsNear)
        {
            ExitShop();
        }
    }

    public void OpenShopPressed(InputEventContext inputEventContext)
    {
        if (!playerIsNear || !inputEventContext.Equals(InputEventContext.DEFAULT) || inShop)
        {
            return;
        }

        inShop = true;

        ShopEntered();
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = true;
            if(buyableMark) buyableMark.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = false;
            if (buyableMark) buyableMark.SetActive(false);
        }
    }

    private void ShopEntered()
    {
        if(contentParent) contentParent.SetActive(true);
        // inputAxisController.enabled = false;
    }

    private void ExitShop()
    {
        if (contentParent) contentParent.SetActive(false);
        inShop = false;
        // inputAxisController.enabled = true;
    }
}

