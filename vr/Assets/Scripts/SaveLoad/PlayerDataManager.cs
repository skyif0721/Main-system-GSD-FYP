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
    private string serverURL = "http://localhost:3000";

    public void SaveGame(string id, string name, int score, int health, int mana, int money, string sceneName, float[] position)
    {
        PlayerData data = new PlayerData();
        data.id = id;
        data.name = name;
        data.score = score;
        data.health = health;
        data.mana = mana;
        data.money = money;
        data.sceneName = sceneName;

        data.position = new float[3];
        data.position[0] = position[0];
        data.position[1] = position[1];
        data.position[2] = position[2];

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
                Debug.Log("Score: " + loadedData.score);
                player.GetComponent<PlayerStats>().currentHealth =  loadedData.health;
                player.GetComponent<PlayerStats>().UpdateHealthUI();
                player.GetComponent<PlayerStats>().currentMana = loadedData.health;
                player.GetComponent<PlayerStats>().UpdateManaUI();
                ShopManager.coins = loadedData.money;
                SceneManager.LoadScene(loadedData.sceneName);

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

}