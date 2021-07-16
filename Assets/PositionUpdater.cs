using Bomber.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionUpdater : MonoBehaviour
{
    [SerializeField] Transform player;


    private void Update()
    {
        transform.position = player.position;
    }
}
