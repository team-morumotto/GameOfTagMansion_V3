using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObstructItem : MonoBehaviourPunCallbacks
{
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField]
    bool isFront = false;
    public GameObject obstructParticle;
    void Start() {
        StartCoroutine(Destroy());
    }

    void Update() {
        if(isFront) {
            transform.position += transform.forward * Time.deltaTime  * moveSpeed * moveSpeed;
        }else{
            transform.position += -transform.forward * Time.deltaTime * moveSpeed * moveSpeed;
        }
    }

    /// <summary>
    /// 5秒後削除.
    /// </summary>
    IEnumerator Destroy() {
        yield return new WaitForSeconds(5.0f);
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider collider) {
        if(collider.CompareTag("Wall") || collider.CompareTag("Player")){
            // Instantiate(obstructParticle, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
    }
}