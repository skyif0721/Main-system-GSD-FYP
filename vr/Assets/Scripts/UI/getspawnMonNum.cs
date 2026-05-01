using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class getspawnMonNum : MonoBehaviour
{
    public Monster Monster;
    public TextMeshProUGUI scoreText;
    private int Spawnnumm = 2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateScoreText();
    }

    private void getspawn() {


        Spawnnumm = Monster.deadCount;
    }

    private void UpdateScoreText()
    {
        getspawn();
        // 4. Access the 'text' property and assign a string value
        scoreText.text = "Spawned monster " + Spawnnumm.ToString();
    }
}