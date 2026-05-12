using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

[System.Serializable]
public class PlayerData
{
    public string id;
    public string name;
    public int score;
    public int health;
    public int mana;
    public int money;
    public string sceneName;
    public float[] position;
}
public class PlayerDataManager : MonoBehaviour
{
    public GameObject player;

    private const string serverURL = "http://localhost:3000";

    private PlayerData _pendingData;
    public int score { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void SaveGame(string id, string name, int score, int health, int mana, int money, string sceneName, float[] position)
    {
        var data = new PlayerData
        {
            id = id,
            name = name,
            score = score,
            health = health,
            mana = mana,
            money = money,
            sceneName = sceneName,

            position = new float[3] { position[0], position[1], position[2] }
        };

        StartCoroutine(SendToServer(data));
    }

    IEnumerator SendToServer(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverURL + "/api/save", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("SAVED to MongoDB!");
            else
                Debug.LogError("Save Error: " + request.error);
        }   
    }

    public void LoadGame(string id)
    {
        StartCoroutine(LoadFromServer(id));
    }

    IEnumerator LoadFromServer(string id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverURL + "/api/load/" + id))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                PlayerData loadedData = JsonUtility.FromJson<PlayerData>(request.downloadHandler.text);

                Debug.Log("LOADED!");
                Debug.Log("Name: " + loadedData.name);
                score = loadedData.score;
                player.GetComponent<PlayerStats>().currentHealth =  loadedData.health;
                player.GetComponent<PlayerStats>().UpdateHealthUI();
                player.GetComponent<PlayerStats>().currentMana = loadedData.health;
                player.GetComponent<PlayerStats>().UpdateManaUI();
                ShopManager.coins = loadedData.money;
                if(!string.Equals(loadedData.sceneName, SceneManager.GetActiveScene().name))
                {
                    _pendingData = loadedData;
                    SceneManager.sceneLoaded += OnSceneLoadedApplyData;
                    SceneManager.LoadScene(loadedData.sceneName);
                }
                else
                {
                    ApplyLoadedData(loadedData);
                }

                    Vector3 savedPos = new(
                        loadedData.position[0],
                        loadedData.position[1],
                        loadedData.position[2]
                    );
                player.transform.position = savedPos;
            }
            else
            {
                Debug.LogError("Load Error: " + request.error);
            }
        }
    }

    private void OnSceneLoadedApplyData(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedApplyData;
        if (_pendingData != null)
        {
            ApplyLoadedData(_pendingData);
            _pendingData = null;
        }
    }

    private void ApplyLoadedData(PlayerData data)
    {
        if (player == null)
        {
            Player instance = Object.FindAnyObjectByType<Player>();
            player = instance.gameObject;
        }
        
        if (player == null)
        {
            Debug.LogWarning("Player not found in the new scene. Make sure a Player exists or spawn one here.");
            return;
        }

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.currentHealth = data.health;
            stats.UpdateHealthUI();

            stats.currentMana = data.mana;
            stats.UpdateManaUI();
        }

        ShopManager.coins = data.money;

        if (data.position != null && data.position.Length >= 3)
        {
            Vector3 savedPos = new Vector3(data.position[0], data.position[1], data.position[2]);
            player.transform.position = savedPos;
        }
    }
}