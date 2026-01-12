using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class getMonNum : MonoBehaviour
{
    public Co monsterdamage;
    public TextMeshProUGUI scoreText;
    private int damage = 0;
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


        damage = monsterdamage.totalDamage;
    }

    private void UpdateScoreText()
    {
        getspawn();
        // 4. Access the 'text' property and assign a string value
        scoreText.text = "Totaldamage " + damage.ToString();
    }
}
