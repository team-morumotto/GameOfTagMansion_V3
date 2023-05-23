using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringBoard : MonoBehaviour
{
    void OnTriggerEnter(Collider collision){
        if(collision.gameObject.tag == "Nige" || collision.gameObject.tag == "Oni"){
            Debug.Log("SpringBoard");
            collision.gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 100, 0), ForceMode.Impulse);
        }
    }
}