using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Score : MonoBehaviour
{
    int score = 0;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    public void AddScore()
    {
        //score++;
        scoreText.text = score.ToString();
    }
}

