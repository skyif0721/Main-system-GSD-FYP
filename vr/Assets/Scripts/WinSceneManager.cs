using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Win scene controller. Shows celebration with coins launching into the air
/// from both sides, coins disappear when they touch the ground.
/// Resets story mode stats after winning.
/// </summary>
public class WinSceneManager : MonoBehaviour
{
    [Header("Celebration Settings")]
    public int coinCount = 40;
    public float coinLaunchForce = 8f;
    public float coinSpread = 3f;
    public float coinSpawnInterval = 0.05f;
    public float groundY = 0f;

    [Header("UI")]
    public Canvas winCanvas;
    public TextMeshProUGUI winText;
    public Button mainMenuButton;
    public Button playAgainButton;

    [Header("Scene Indices")]
    public int mainMenuSceneIndex = 0;
    public int mainGameSceneIndex = 1;

    [Header("Coin Visual")]
    public Color coinColor = new Color(1f, 0.85f, 0.2f, 1f);
    public float coinSize = 0.15f;

    private List<GameObject> _coins = new List<GameObject>();

    void Start()
    {
        // Reset story stats on win
        ResetStoryStats();

        CreateWinUI();
        StartCoroutine(CoinCelebration());

        // Play menu BGM
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayMenuBGM();
    }

    void Update()
    {
        // Check coins touching ground and destroy them
        for (int i = _coins.Count - 1; i >= 0; i--)
        {
            if (_coins[i] == null)
            {
                _coins.RemoveAt(i);
                continue;
            }

            if (_coins[i].transform.position.y <= groundY)
            {
                // Spawn small particle burst
                SpawnCoinPoof(_coins[i].transform.position);
                Destroy(_coins[i]);
                _coins.RemoveAt(i);
            }
        }
    }

    void CreateWinUI()
    {
        GameObject canvasGO = new GameObject("WinCanvas");
        winCanvas = canvasGO.AddComponent<Canvas>();
        winCanvas.renderMode = RenderMode.WorldSpace;
        winCanvas.sortingOrder = 100;

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(800, 600);
        canvasRT.localScale = Vector3.one * 0.005f;

        Camera cam = Camera.main;
        if (cam != null)
        {
            canvasGO.transform.position = cam.transform.position + cam.transform.forward * 2.5f;
            canvasGO.transform.rotation = Quaternion.LookRotation(
                canvasGO.transform.position - cam.transform.position);
        }
        else
        {
            canvasGO.transform.position = new Vector3(0, 1.5f, 2.5f);
        }

        canvasGO.AddComponent<CanvasScaler>();
        // Use TrackedDeviceGraphicRaycaster for VR ray interaction
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.05f, 0.02f, 0.9f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Win text
        GameObject textGO = new GameObject("WinText");
        textGO.transform.SetParent(canvasGO.transform, false);
        winText = textGO.AddComponent<TextMeshProUGUI>();
        winText.text = "VICTORY!";
        winText.fontSize = 80;
        winText.fontStyle = FontStyles.Bold;
        winText.color = new Color(1f, 0.85f, 0.2f);
        winText.alignment = TextAlignmentOptions.Center;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0.6f);
        textRT.anchorMax = new Vector2(1, 0.9f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // Subtitle
        GameObject subGO = new GameObject("Subtitle");
        subGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI subText = subGO.AddComponent<TextMeshProUGUI>();
        subText.text = "You have defeated the Final Boss!\nThe realm is saved!";
        subText.fontSize = 28;
        subText.color = Color.white;
        subText.alignment = TextAlignmentOptions.Center;
        RectTransform subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0, 0.4f);
        subRT.anchorMax = new Vector2(1, 0.6f);
        subRT.offsetMin = new Vector2(40, 0);
        subRT.offsetMax = new Vector2(-40, 0);

        // Play Again button
        GameObject playGO = CreateUIButton(canvasGO.transform, "PlayAgainBtn",
            "Play Again", new Vector2(0, 0.2f), new Vector2(1, 0.33f),
            new Color(0.1f, 0.55f, 0.2f));
        playAgainButton = playGO.GetComponent<Button>();
        playAgainButton.onClick.AddListener(PlayAgain);

        // Main Menu button
        GameObject menuGO = CreateUIButton(canvasGO.transform, "MainMenuBtn",
            "Main Menu", new Vector2(0, 0.05f), new Vector2(1, 0.18f),
            new Color(0.4f, 0.15f, 0.15f));
        mainMenuButton = menuGO.GetComponent<Button>();
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    GameObject CreateUIButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = color;
        Button btn = btnGO.AddComponent<Button>();
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = anchorMin;
        btnRT.anchorMax = anchorMax;
        btnRT.offsetMin = new Vector2(200, 0);
        btnRT.offsetMax = new Vector2(-200, 0);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 30;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btnGO;
    }

    IEnumerator CoinCelebration()
    {
        yield return new WaitForSeconds(0.5f);

        Camera cam = Camera.main;
        Vector3 center = cam != null ? cam.transform.position : Vector3.zero;

        for (int i = 0; i < coinCount; i++)
        {
            // Alternate left and right sides
            bool leftSide = i % 2 == 0;
            float xOffset = leftSide ? -coinSpread : coinSpread;

            Vector3 spawnPos = center + new Vector3(xOffset, 0.5f, 1.5f);
            spawnPos += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.3f), Random.Range(-0.3f, 0.3f));

            GameObject coin = CreateCoin(spawnPos);
            Rigidbody rb = coin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Launch upward and to the side
                float sideForce = leftSide ? -2f : 2f;
                rb.velocity = new Vector3(sideForce + Random.Range(-1f, 1f),
                    coinLaunchForce + Random.Range(-1f, 2f),
                    Random.Range(-1f, 1f));
                rb.angularVelocity = Random.insideUnitSphere * 10f;
            }

            _coins.Add(coin);
            yield return new WaitForSeconds(coinSpawnInterval);
        }
    }

    GameObject CreateCoin(Vector3 position)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "CelebrationCoin";
        coin.transform.position = position;
        coin.transform.localScale = new Vector3(coinSize, coinSize * 0.1f, coinSize);

        // Remove default collider, add smaller one
        Object.Destroy(coin.GetComponent<Collider>());

        Rigidbody rb = coin.AddComponent<Rigidbody>();
        rb.mass = 0.1f;
        rb.drag = 0.2f;

        Renderer r = coin.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = coinColor;
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.8f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.9f);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", coinColor * 0.5f);
            mat.EnableKeyword("_EMISSION");
        }
        r.material = mat;

        // Auto-destroy after 10 seconds as safety
        Destroy(coin, 10f);

        return coin;
    }

    void SpawnCoinPoof(Vector3 pos)
    {
        GameObject psGO = new GameObject("CoinPoof");
        psGO.transform.position = pos;
        ParticleSystem ps = psGO.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 1.5f;
        main.startSize = 0.05f;
        main.startColor = coinColor;
        main.maxParticles = 15;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var psr = psGO.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = coinColor;
            psr.material = mat;
        }

        ps.Play();
        Destroy(psGO, 1.5f);
    }

    void ResetStoryStats()
    {
        // Reset coins
        ShopManager.coins = 0;
        PlayerPrefs.SetInt("SavedCoins", 0);

        // Reset weapon unlocks
        for (int i = 0; i < 12; i++)
        {
            PlayerPrefs.DeleteKey("WeaponUnlocked_" + i);
        }

        // Reset final boss ticket
        PlayerPrefs.DeleteKey("HasFinalBossTicket");

        PlayerPrefs.Save();
        Debug.Log("[WinScene] Story mode stats reset!");
    }

    public void PlayAgain()
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(mainGameSceneIndex);
        else
            SceneManager.LoadScene(mainGameSceneIndex);
    }

    public void GoToMainMenu()
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(mainMenuSceneIndex);
        else
            SceneManager.LoadScene(mainMenuSceneIndex);
    }
}
