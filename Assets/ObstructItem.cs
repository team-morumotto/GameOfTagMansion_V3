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
            transform.position += (transform.forward / 60) * moveSpeed;
        }else{
            transform.position += (-transform.forward / 60) * moveSpeed;
        }
    }

    void OnDestroy() {
		Instantiate(obstructParticle, transform.position, transform.rotation);
	}

    IEnumerator Destroy() {
        yield return new WaitForSeconds(10.0f);
        Destroy(this.gameObject);
    }
}
