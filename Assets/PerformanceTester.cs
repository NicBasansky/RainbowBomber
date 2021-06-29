using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTester : MonoBehaviour
{
    const int numTests = 5000;

    void PerformanceTestA()
    {
        for (int i = 0; i < numTests; i++)
        {
            //var t = (Transform)GetComponent("Transform");
            //GameObject.FindWithTag("Player");
        }
    }

    void PerformanceTestB()
    {
        for (int i = 0; i < numTests; i++)
        {
            //var t = GetComponent("Transform").transform;
            //GameObject.FindGameObjectWithTag("Player");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformanceTestA();
            PerformanceTestB();
        }
    }
}
