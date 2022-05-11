using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCheckpointAfterPassing : MonoBehaviour
{
    private void OnTriggerExit(Collider collision)
    {
        if(collision.collider.CompareTag())
    }
}
