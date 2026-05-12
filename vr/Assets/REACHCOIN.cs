using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class REACHCOIN : MonoBehaviour
{
    // Start is called before the first frame update
    public string sceneName;
    public ShopManager bossStats; // Drag the Boss Monster here in the Inspector

    void Update()
    {
        // Check if the boss exists and if its health has dropped to 0 or less
        if (bossStats != null && bossStats.coinreaching <= 1000)
        {
            LoadScene();
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
