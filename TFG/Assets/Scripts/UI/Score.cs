using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Score : NetworkBehaviour
{
    public NetworkVariable<int> score = new();
    [SerializeField]
    private TextMeshProUGUI scoreText;

    public override void OnNetworkDespawn()
    {
        score.OnValueChanged -= NewScore;
    }

    public override void OnNetworkSpawn()
    {
        score.OnValueChanged += NewScore;
    }

    private void NewScore(int previousValue, int newValue)
    {
        score.Value = newValue;
        scoreText.text = score.Value.ToString();
    }
}

