using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerScript : MonoBehaviour
{
    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag == "Nige")
        {
            Debug.Log("Nige");
        }
    }
}
