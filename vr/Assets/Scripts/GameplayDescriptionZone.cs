using UnityEngine;
using TMPro;

/// <summary>
/// A zone in the tutorial that shows the gameplay description/rules.
/// Creates a world-space board with game info when the player enters.
/// </summary>
public class GameplayDescriptionZone : MonoBehaviour
{
    [Header("Board Position")]
    public Vector3 boardPosition = new Vector3(-5f, 1.5f, 3f);
    public Quaternion boardRotation = Quaternion.Euler(0f, 150f, 0f);

    [Header("Content")]
    [TextArea(5, 15)]
    public string gameDescription = @"<color=#FFD700><b>GAMEPLAY GUIDE</b></color>

<color=#66E6FF><b>Combat Gestures:</b></color>
• <color=#66E6FF>Rapier</color> - Thrust forward to fire a projectile (60 dmg)
• <color=#FFF033>Split</color> - Chop downward for cone damage (45 dmg)
• <color=#4DB8FF>Block</color> - Both hands up to become invulnerable


<color=#00FF88><b>Economy:</b></color>
• Defeat monsters to earn coins
• Buy weapons and potions at the shop
• Collect 1000 coins to buy a Final Boss ticket

<color=#FF6666><b>Weapons:</b></color>
• Weapons unlock in order (cheapest first)
• Each weapon has damage and durability stats
• Broken weapons must be re-spawned from the shop

<color=#FFD700><b>Goal:</b></color>
Defeat the Final Boss to win the game!
Good luck, warrior!";

    private GameObject _board;

    void Start()
    {
        CreateBoard();
    }

    void CreateBoard()
    {
        _board = new GameObject("GameplayDescBoard");
        _board.transform.position = boardPosition;
        _board.transform.rotation = boardRotation;

        // Background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "BG";
        bg.transform.SetParent(_board.transform);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.02f);
        bg.transform.localRotation = Quaternion.identity;
        bg.transform.localScale = new Vector3(2.8f, 3.5f, 1f);
        Object.Destroy(bg.GetComponent<Collider>());

        Material bgMat = new Material(Shader.Find("Standard"));
        bgMat.SetFloat("_Mode", 3f);
        bgMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        bgMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        bgMat.SetInt("_ZWrite", 0);
        bgMat.EnableKeyword("_ALPHABLEND_ON");
        bgMat.renderQueue = 3000;
        bgMat.color = new Color(0.03f, 0.05f, 0.08f, 0.92f);
        bg.GetComponent<Renderer>().material = bgMat;

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(_board.transform);
        titleGO.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        titleGO.transform.localRotation = Quaternion.identity;
        TextMeshPro titleTmp = titleGO.AddComponent<TextMeshPro>();
        titleTmp.text = "How To Play";
        titleTmp.fontSize = 0.8f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(1f, 0.85f, 0.3f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.rectTransform.sizeDelta = new Vector2(2.5f, 0.5f);


        // Description text
        GameObject descGO = new GameObject("Description");
        descGO.transform.SetParent(_board.transform);
        descGO.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        descGO.transform.localRotation = Quaternion.identity;
        TextMeshPro descTmp = descGO.AddComponent<TextMeshPro>();
        descTmp.text = gameDescription;
        descTmp.fontSize = 0.32f;
        descTmp.color = new Color(0.85f, 0.88f, 0.95f);
        descTmp.alignment = TextAlignmentOptions.Left;
        descTmp.rectTransform.sizeDelta = new Vector2(2.4f, 2.8f);
        descTmp.enableWordWrapping = true;
        descTmp.richText = true;

    }
}
