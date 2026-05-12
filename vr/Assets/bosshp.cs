using UnityEngine;
using UnityEngine.SceneManagement;

public class bosshp : MonoBehaviour
{
    public string sceneName;
    public MonsterStat bossStats; // Drag the Boss Monster here in the Inspector

    private bool _defeated = false;

    void Update()
    {
        // Check if the boss exists and if its health has dropped to 0 or less
        if (!_defeated && bossStats != null && bossStats.health <= 0)
        {
            _defeated = true;
            LoadScene();
        }
    }

    public void LoadScene()
    {
        // Use GameLoopManager if available for proper scene flow
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.OnBossDefeated();
            return;
        }

        // Fallback: use SceneTransitionManager or direct load
        if (SceneTransitionManager.singleton != null)
        {
            // Win scene is index 5
            SceneTransitionManager.singleton.GoToSceneAsync(5);
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(5);
        }
    }
}
