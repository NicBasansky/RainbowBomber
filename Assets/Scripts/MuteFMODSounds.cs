using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteFMODSounds : MonoBehaviour
{
    public bool mute;
    string masterBusString = "Bus:/SoundEffects";
    FMOD.Studio.Bus masterBus;

    private void Start()
    {
        masterBus = FMODUnity.RuntimeManager.GetBus(masterBusString);
        MuteMasterBus();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mute = !mute;
            MuteMasterBus();
        }
    }

    private void MuteMasterBus()
    {
          
        masterBus.setMute(mute);
    }
}
