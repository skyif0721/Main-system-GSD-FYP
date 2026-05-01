using System.Collections;
using System.Collections.Generic;
using System.IO; // 為了使用 Directory
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using PDollarGestureRecognizer;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.Events;

public class MovementRecognizer : MonoBehaviour
{
    public XRNode inputSource;
    public InputHelpers.Button inputButton;
    public float inputThreshold = 0.1f;
    public Transform movementSource;

    public float newPositionThresholdDistance = 0.05f;
    public GameObject debugCubePrefab;
    public bool creationMode = true;
    public string newGestureName;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecongnized;

    private List<Gesture> trainingSet = new List<Gesture>();
    private bool isMoving = false;
    private List<Vector3> positionsList = new List<Vector3>();

    public float recognitionThreshold = 0.9f;

    // Start is called before the first frame update
    void Start()
    {
        // 取得所有已儲存的 .xml 手勢檔案路徑
        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath, "*.xml");

        foreach (var item in gestureFiles)
        {
            // 將檔案讀取並加入訓練集 (對應截圖中的邏輯)
            trainingSet.Add(GestureIO.ReadGestureFromFile(item));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 依照影片截圖，使用舊版的 InputHelpers
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputSource), inputButton, out bool isPressed, inputThreshold);

        if (!isMoving && isPressed)
        {
            StartMovement();
        }
        else if (isMoving && !isPressed)
        {
            EndMovement();
        }
        else if (isMoving && isPressed)
        {
            UpdateMovement();
        }
    }

    void StartMovement()
    {
        Debug.Log("Start Movement");
        isMoving = true;
        positionsList.Clear();
        positionsList.Add(movementSource.position);

        if (debugCubePrefab)
        {
            Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3);
        }
    }

    void EndMovement()
    {
        Debug.Log("End Movement");
        isMoving = false;

        Point[] pointArray = new Point[positionsList.Count];

        for (int i = 0; i < positionsList.Count; i++)
        {

            Vector2 ScreenPoint = Camera.main.WorldToScreenPoint(positionsList[i]);
            pointArray[i] = new Point(ScreenPoint.x, ScreenPoint.y, 0);

        }

        Gesture newGesture = new Gesture(pointArray);


        if (creationMode)
        {
            newGesture.Name = newGestureName;
            trainingSet.Add(newGesture);

            string fileName = Application.persistentDataPath + "/" + newGestureName + ".xml";
            // 這裡傳入的是 Point[]，現在類型應該已經匹配
            GestureIO.WriteGesture(pointArray, newGestureName, fileName);
        }
        //recognize
        else
        {
            Result result = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());
            Debug.Log(result.GestureClass + result.Score);

            if (result.Score > recognitionThreshold)
            {
                OnRecongnized.Invoke(result.GestureClass);
            }
        }





    }


    void UpdateMovement()
        {
            Vector3 lastPosition = positionsList[positionsList.Count - 1];

            if (Vector3.Distance(movementSource.position, lastPosition) > newPositionThresholdDistance)
            {
                positionsList.Add(movementSource.position);
                if (debugCubePrefab)
                {
                    Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3);
            }
         }
    }
   
}
