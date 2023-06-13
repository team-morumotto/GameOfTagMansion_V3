using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateSample : MonoBehaviour
{
    delegate void MyDel(int x, int y);
    void aa(int x, int y) {
        print(x);
        print(y);
    }

    void bb(int x, int y) {
        print(x + y);
    }

    delegate void VoidDel();

    void aa() {
        print(2);
    }

    void bb() {
        print(3);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G)) {
            MyDel md;
            md = aa;
            md(1,5);
            md = bb;
            md(1,5);
        }

        if(Input.GetKeyDown(KeyCode.H)) {
            VoidDel vd;
            vd = aa;
            vd();
            vd = bb;
            vd();
        }
    }
}