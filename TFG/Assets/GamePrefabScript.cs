using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePrefabScript : MonoBehaviour
{
    private GameObject objectToActivate;
    private GameObject objectToDesactivate;
    public EventHandler<string> onClicked;
    private AudioSource audioSource;
    void Start()
    {
        Button btn = GetComponent<Button>();
        audioSource = FindAnyObjectByType<AudioSource>();
        if (btn != null)
        {
            btn.onClick.AddListener(ActivateObject);
        }
    }


    public void SetObjectToActivate(GameObject obj)
    {
        objectToActivate = obj;
    }
    public void SetObjectToDesactivate(GameObject obj)
    {
        objectToDesactivate = obj;
    }

    void ActivateObject()
    {
        if (objectToActivate != null)
        {
            onClicked?.Invoke(this, GetComponentInChildren<TextMeshProUGUI>().text);
            objectToActivate.SetActive(true);
            objectToDesactivate.SetActive(false);
            
        }
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
