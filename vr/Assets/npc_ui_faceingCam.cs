using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class npc_ui_faceingCam : MonoBehaviour
{
    public enum FacingTarget { Camera, Player, Hybrid }

    [Header("Components")]
    [SerializeField] private Canvas canvas;

    [Header("Behavior")]
    public Transform player;
    public Camera uiCamera;
    public FacingTarget faceTarget = FacingTarget.Camera;

    Action targetAction;

    private void Start()
    {
        if (!uiCamera) uiCamera = Camera.main;
        UpdateTargetAction();
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
        if (canvas.gameObject.activeSelf) targetAction?.Invoke();
    }
}
