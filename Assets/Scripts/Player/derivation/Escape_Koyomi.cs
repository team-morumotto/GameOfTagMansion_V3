/*
    2023/03/10 Kobayashi Atsuki.
    GameOfTagMansionにおける逃げの水鏡こよみの固有性能.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escape_Koyomi : PlayerEscape
{
    private float der_walkSpeed = 10.0f;
    private float der_runSpeed = 15.0f;
    void Start()
    {
        walkSpeed = der_walkSpeed;
        runSpeed = der_runSpeed;
    }
}