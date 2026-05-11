using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the overall game loop. Handles scene transitions, stat resets,
/// and ensures the game can loop properly.
/// Persists across scenes (DontDestroyOnLoad).
/// 
/// Scene flow:
///   0: Main Menu (Start Scene)
///   1: Shop/Training (shop-training)
///   2: Tutorial
///   3: Final Boss
///   4: Death Scene
///   5: Win Scene
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("Scene Indices")]
    public int mainMenuScene = 0;
    public int shopTrainingScene = 1;
    public int tutorialScene = 2;
    public int finalBossScene = 3;
    public int deathScene = 4;
    public int winScene = 5;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int idx = scene.buildIndex;
        Debug.Log($"[GameLoop] Scene loaded: {scene.name} (index {idx})");

        // Set appropriate BGM
        if (GameAudioManager.Instance != null)
        {
            if (idx == mainMenuScene || idx == tutorialScene)
                GameAudioManager.Instance.PlayMenuBGM();
            else if (idx == shopTrainingScene)
                GameAudioManager.Instance.PlayExploreBGM();
            else if (idx == finalBossScene)
                GameAudioManager.Instance.PlayBossBGM();
            else if (idx == deathScene || idx == winScene)
                GameAudioManager.Instance.StopAllBGM();
        }
    }

    /// <summary>
    /// Called when the player dies. Transitions to death scene.
    /// </summary>
    public void OnPlayerDeath()
    {
        Debug.Log("[GameLoop] Player died! Going to death scene.");

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(deathScene);
        else
            SceneManager.LoadScene(deathScene);
    }

    /// <summary>
    /// Called when the boss is defeated. Transitions to win scene.
    /// </summary>
    public void OnBossDefeated()
    {
        Debug.Log("[GameLoop] Boss defeated! Going to win scene.");

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(winScene);
        else
            SceneManager.LoadScene(winScene);
    }

    /// <summary>
    /// Reset all story mode stats for a fresh playthrough.
    /// </summary>
    public static void ResetAllStats()
    {
        ShopManager.coins = 0;
        PlayerPrefs.SetInt("SavedCoins", 0);

        for (int i = 0; i < 12; i++)
            PlayerPrefs.DeleteKey("WeaponUnlocked_" + i);

        PlayerPrefs.DeleteKey("HasFinalBossTicket");
        PlayerPrefs.Save();

        Debug.Log("[GameLoop] All story stats reset!");
    }
}
