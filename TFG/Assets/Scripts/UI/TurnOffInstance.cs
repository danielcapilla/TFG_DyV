using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffInstance : MonoBehaviour
{
    private MeshRenderer visual;

    private void Start()
    {
        visual = GetComponent<MeshRenderer>();
        if (!HostManager.activarEscena)
        {
            visual.enabled = false;
        }

    }
}
