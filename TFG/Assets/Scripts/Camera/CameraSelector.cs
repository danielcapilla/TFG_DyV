using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSelector : MonoBehaviour
{
    [SerializeField] List<CinemachineVirtualCamera> cameras;
    CinemachineVirtualCamera currentCamera; 
    int currentViewingCameraID = 0;
    public EventHandler<OnCameraChangeEventArgs> OnCameraChange;
    public class OnCameraChangeEventArgs : System.EventArgs
    {
        public int cameraID;
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (CinemachineVirtualCamera cam in cameras) 
        {
            cam.gameObject.SetActive(false);
        }
        cameras[0].gameObject.SetActive(true);
        currentCamera = cameras[0];
    }


    public void ActivateCamera(int id) 
    {
        currentCamera.gameObject.SetActive(false);
        cameras[id].gameObject.SetActive(true);
        currentCamera = cameras[id];
    }

    public void NextCamera() 
    {
        currentViewingCameraID++;
        if (currentViewingCameraID >= cameras.Count) currentViewingCameraID = 0;
        OnCameraChange?.Invoke(this, new OnCameraChangeEventArgs { cameraID = currentViewingCameraID});
        ActivateCamera(currentViewingCameraID);
    }

    public void PreviousCamera() 
    {
        currentViewingCameraID--;
        if (currentViewingCameraID < 0) currentViewingCameraID = cameras.Count-1;
        OnCameraChange?.Invoke(this, new OnCameraChangeEventArgs { cameraID = currentViewingCameraID });
        ActivateCamera(currentViewingCameraID);
    }
}
