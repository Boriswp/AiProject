using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCheckpointAfterPassing : MonoBehaviour
{
    private void OnTriggerExit(Collider collision)
    {
        if(collision.gameObject.CompareTag("BotGA")) return;
        if (collision.transform.parent.name != "Body") return;
        GetComponent<BoxCollider>().enabled = false;
    }
}
