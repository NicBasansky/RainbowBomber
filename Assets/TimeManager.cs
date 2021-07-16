using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float slowdownFactor = 0.05f;
    public float slowdownDuration = 2.0f;
    public float releaseTime = 0.70f;
    bool released = true;

    private void Update()
    {
        if (!released)
        {
            Time.timeScale += (1f / slowdownDuration) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0, 1);
            // if slowdownDuration is 2, then 1 / 2 = 0.5 increase to timeScale each second.
            // unscaledDeltaTime is not affected when changing the time 
        }
        else
        {
            Time.timeScale += (1f / releaseTime) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0, 1);
        }
        // when you slowdown, try lowering the pitch of your audio and increase the pitch as you increase the time scale
    }

    public void BulletTime()
    {
        released = false;
        Time.timeScale = slowdownFactor;
    }

    public void ReleaseBulletTime()
    {
        released = true;
    }
}
