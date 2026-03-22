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
        contentParent.SetActive(false);
        // inputActions = new @XRIDefaultInputActions();
        inShop = false;
    }

    private void Start()
    {
        if (!uiCamera) uiCamera = Camera.main;

        // inputAxisController = player.GetComponentInChildren<CinemachineInputAxisController>();

        UpdateTargetAction();
    }

    private void OnEnable()
    {
        ShopExitButton.onClick.AddListener(ExitShop);

        GameEventsManager.instance.inputEvents.onOpenShopPressed += OpenShopPressed;

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = uiCamera;

        if (ShopExitButton)
        {
            ShopExitButton.onClick.RemoveAllListeners();
            ShopExitButton.onClick.AddListener(() => ExitShop());
        }

    }

    private void OnDisable()
    {
        ShopExitButton.onClick.RemoveListener(ExitShop);

        GameEventsManager.instance.inputEvents.onOpenShopPressed -= OpenShopPressed;
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
        if (canvas.gameObject.activeSelf) targetAction?.Invoke();

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
            buyableMark.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = false;
            buyableMark.SetActive(false);
        }
    }

    private void ShopEntered()
    {
        contentParent.SetActive(true);
        // inputAxisController.enabled = false;
    }

    private void ExitShop()
    {
        contentParent.SetActive(false);
        inShop = false;
        // inputAxisController.enabled = true;
    }
}

