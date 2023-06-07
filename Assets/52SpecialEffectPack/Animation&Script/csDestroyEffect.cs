using UnityEngine;
using System.Collections;

public class csDestroyEffect : MonoBehaviour {

    void Start() {
        StartCoroutine(Destroy());
    }

    IEnumerator Destroy() {
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}
