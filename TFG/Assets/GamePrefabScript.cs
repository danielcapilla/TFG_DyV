using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePrefabScript : MonoBehaviour
{
    private GameObject objectToActivate;
    private GameObject objectToDesactivate;

    void Start()
    {
        Button btn = GetComponent<Button>();
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
        Debug.Log("Activating object");
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            objectToDesactivate.SetActive(false);
        }
    }
}
