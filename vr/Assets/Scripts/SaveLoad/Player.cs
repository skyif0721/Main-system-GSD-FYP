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
        this.id = "player_001";
        this.playerName = "PlayerName";
        this.score = ShopManager.coins / 10;
        this.health = player.GetComponent<PlayerStats>().currentHealth;
        this.mana = player.GetComponent<PlayerStats>().currentMana;
        this.money = ShopManager.coins;
        this.sceneName = sceneName = SceneManager.GetActiveScene().name;

        // Save player position
        this.position = new float[3];
        this.position[0] = player.transform.position.x;
        this.position[1] = player.transform.position.y;
        this.position[2] = player.transform.position.z;

        playerDataManager.GetComponent<PlayerDataManager>().SaveGame(id, name, score, health, mana, money, sceneName, position);
    }

    public void PlayerLoad()
    {
        playerDataManager.GetComponent<PlayerDataManager>().LoadGame(id);
    }
}
