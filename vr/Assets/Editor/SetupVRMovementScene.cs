using UnityEngine;
using UnityEditor;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class SetupVRMovementScene
{
    public static void Execute()
    {
        // Create Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = Vector3.zero;

        // Instantiate XR Origin
        GameObject xrOriginPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/XRI_Examples/Global/Prefabs/Complete XR Origin Set Up Variant.prefab");
        GameObject xrOrigin = null;
        if (xrOriginPrefab != null)
        {
            xrOrigin = PrefabUtility.InstantiatePrefab(xrOriginPrefab) as GameObject;
            xrOrigin.transform.position = Vector3.zero;
        }
        else
        {
            Debug.LogError("XR Origin prefab not found!");
            return;
        }

        // Find Right Controller
        Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");
        if (rightController == null)
        {
            // Try another path
            rightController = xrOrigin.transform.Find("Camera Offset/RightHand Controller");
        }
        if (rightController == null)
        {
            // Try finding by name recursively
            Transform[] allTransforms = xrOrigin.GetComponentsInChildren<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name.Contains("Right Controller") || t.name.Contains("RightHand Controller"))
                {
                    rightController = t;
                    break;
                }
            }
        }

        // Create Debug Cube Prefab
        GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        debugCube.GetComponent<Renderer>().sharedMaterial.color = Color.red;
        GameObject.DestroyImmediate(debugCube.GetComponent<Collider>());
        
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        }
        GameObject debugCubePrefab = PrefabUtility.SaveAsPrefabAsset(debugCube, "Assets/Prefabs/DebugCube.prefab");
        GameObject.DestroyImmediate(debugCube);

        // Create Movement Recognizer
        GameObject movementRecognizerObj = new GameObject("MovementRecognizer");
        MovementRecognizer recognizer = movementRecognizerObj.AddComponent<MovementRecognizer>();
        recognizer.inputSource = XRNode.RightHand;
        recognizer.inputButton = InputHelpers.Button.Trigger;
        recognizer.movementSource = rightController;
        recognizer.debugCubePrefab = debugCubePrefab;
        recognizer.creationMode = false; // Set to false to recognize gestures

        // Create UI Canvas for Output
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.transform.position = new Vector3(0, 1.5f, 2f);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        canvasObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 200);

        GameObject textObj = new GameObject("OutputText");
        textObj.transform.SetParent(canvasObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = "Perform a gesture...";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 200);

        // Create a script to handle the event
        GameObject eventHandlerObj = new GameObject("EventHandler");
        GestureEventHandler handler = eventHandlerObj.AddComponent<GestureEventHandler>();
        handler.outputText = text;

        // Hook up the event
        if (recognizer.OnRecongnized == null)
        {
            recognizer.OnRecongnized = new MovementRecognizer.UnityStringEvent();
        }
        UnityEditor.Events.UnityEventTools.AddPersistentListener(recognizer.OnRecongnized, handler.OnGestureRecognized);

        Debug.Log("Scene setup complete.");
    }
}
