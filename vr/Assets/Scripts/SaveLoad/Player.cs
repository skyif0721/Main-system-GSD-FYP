using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public string id;
    public string playerName;
    public int score;
    public int health;
    public int mana;
    public int money;
    public string sceneName;
    public float[] position;

    public GameObject player;
    public GameObject playerDataManager;

    public void PlayerSave()
    {
        // Ensure id exists and persist it so loads work after restart
        if (string.IsNullOrEmpty(id))
            id = "player_001";
        PlayerPrefs.SetString("player_id", id);
        PlayerPrefs.Save();

        this.score = ShopManager.coins * 10;
        this.health = player.GetComponent<PlayerStats>().currentHealth;
        this.mana = player.GetComponent<PlayerStats>().currentMana;
        this.money = ShopManager.coins;
        this.sceneName = SceneManager.GetActiveScene().name;

        this.position = new float[3];
        this.position[0] = player.transform.position.x;
        this.position[1] = player.transform.position.y;
        this.position[2] = player.transform.position.z;

        // Pass playerName, not 'name' (MonoBehaviour/GameObject name)
        playerDataManager.GetComponent<PlayerDataManager>()
            .SaveGame(id, playerName, score, health, mana, money, sceneName, position);
    }

    public void Start()
    {
        // Restore id on session start so loading works even after relaunch
        if (string.IsNullOrEmpty(id))
            id = PlayerPrefs.GetString("player_id", "player_001");
    }

    public void PlayerDataLoad()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = PlayerPrefs.GetString("player_id", "player_001");
        }
        playerDataManager.GetComponent<PlayerDataManager>().LoadGame(id);
    }

    public void PlayerSceneLoad()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = PlayerPrefs.GetString("player_id", "player_001");
        }
        playerDataManager.GetComponent<PlayerDataManager>().LoadScene(id);
    }
}
