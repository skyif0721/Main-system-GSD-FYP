using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    public enum FacingTarget { Camera, Player, Hybrid }

    [Header("Components")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Button ShopExitButton;
    [SerializeField] private Button enterShop;
    [SerializeField] private GameObject buyableMark;

    [Header("Behavior")]
    public Transform player;
    public Camera uiCamera;
    public FacingTarget faceTarget = FacingTarget.Camera;

    private bool playerIsNear = false;
    private bool inShop = false;

    Action targetAction;

    private void Start()
    {
        if (uiCamera == null) uiCamera = Camera.main;
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (contentParent != null) contentParent.SetActive(false);

        inShop = false;
        UpdateTargetAction();
    }

    private void OnEnable()
    {
        ShopExitButton.onClick.AddListener(ExitShop);

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            if (uiCamera == null) uiCamera = Camera.main;
            canvas.worldCamera = uiCamera;
        }

        if (ShopExitButton != null)
        {
            ShopExitButton.onClick.RemoveAllListeners();
            ShopExitButton.onClick.AddListener(() => ExitShop());
        }

    }

    private void OnDisable()
    {
        if (ShopExitButton != null)
            ShopExitButton.onClick.RemoveListener(ExitShop);
    }

    private void UpdateTargetAction()
    {
        targetAction = faceTarget switch
        {
            FacingTarget.Camera => FaceCamera,
            _ => null
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

    public void OpenShopPressed()
    {
        if (!playerIsNear || inShop)
        {
            return;
        }

        ShopEntered();
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player") || otherCollider.name.Contains("XR Origin") || otherCollider.GetComponentInParent<PlayerStats>() != null)
        {
            playerIsNear = true;
            if (buyableMark) buyableMark.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player") || otherCollider.name.Contains("XR Origin") || otherCollider.GetComponentInParent<PlayerStats>() != null)
        {
            playerIsNear = false;
            if (buyableMark) buyableMark.SetActive(false);
        }
    }

    private void ShopEntered()
    {
        if (contentParent) contentParent.SetActive(true);
        inShop = true;
    }

    private void ExitShop()
    {
        if (contentParent) contentParent.SetActive(false);
        inShop = false;
    }
}

