using UnityEngine;
using UnityEngine.SceneManagement;

public class bosshp : MonoBehaviour
{
    public string sceneName;
    public MonsterStat bossStats; // Drag the Boss Monster here in the Inspector

    void Update()
    {
        // Check if the boss exists and if its health has dropped to 0 or less
        if (bossStats != null && bossStats.health <= 0)
        {
            LoadScene();
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}