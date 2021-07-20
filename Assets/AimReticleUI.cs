using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimReticleUI : MonoBehaviour
{
    [SerializeField] GameObject aimUIGo;
    [SerializeField] Image reticle;

    private void Start()
    {
        aimUIGo.SetActive(false);
    }

    public void EnableAimUI(bool enabled)
    {
        aimUIGo.SetActive(enabled);
    }

    public void SetHasTarget(bool hasTarget)
    {
        if (hasTarget)
        {
            reticle.color = Color.red;
        }
        else
        {
            reticle.color = Color.white;
        }
    }
}
