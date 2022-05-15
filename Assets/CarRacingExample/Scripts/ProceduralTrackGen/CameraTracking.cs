using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    public GameObject[] cameras;
    private int curr=0;
    
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
            if(cameras.Length<=curr-1) return;
            curr++;
            OnNextCameraClick();
        }
        else
        {
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
