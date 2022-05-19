using System;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    public GameObject[] cameras;
    private int curr=0;

    private void Awake()
    {
       Init();
    }

    public void Init()
    {
        cameras = GameObject.FindGameObjectsWithTag("Camera");
        foreach (var camera in cameras)
        { 
             camera.SetActive(false);
        }
        cameras[curr].SetActive(true); 
    }


    public void OnButtonClick(bool next)
    {

        if (next)
        {
            Debug.Log("next");
            if(cameras.Length<=curr-1) return;
            curr++;
            OnNextCameraClick();
        }
        else
        {
            Debug.Log("prev");
            if(0>=curr) return;
            curr--;
            OnPreviousCameraClick();
        }
    }
    
    private void OnNextCameraClick()
    {
        cameras[curr-1].SetActive(false); 
        cameras[curr].SetActive(true); 
    }

    private void OnPreviousCameraClick()
    {
        cameras[curr+1].SetActive(false); 
        cameras[curr].SetActive(true); 
    }
}
