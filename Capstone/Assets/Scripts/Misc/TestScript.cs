﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void testInt2Uint()
    {
        Debug.Log(MathHelp.sigfigify(9, 2));
        Debug.Log(MathHelp.sigfigify(2653156231, 7));
    }
}
