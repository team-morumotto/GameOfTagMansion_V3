using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManeger : MonoBehaviour
{
    public bool isSpeedUp = false;
    private float WaitTime = 0;
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

    public void kinokoOn(){
        StartCoroutine(UpSpeed());
    }

    IEnumerator UpSpeed(){
        isSpeedUp = true;
        WaitTime = 10f;
        yield return new WaitForSeconds(WaitTime);
        isSpeedUp = false;
    }
}
