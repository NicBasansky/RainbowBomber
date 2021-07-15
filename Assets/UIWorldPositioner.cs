using Bomber.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldPositioner : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] PlayerController controller;

    
    void Update()
    {
        transform.position = player.position;
    }
}
