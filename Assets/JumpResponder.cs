using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpResponder : MonoBehaviour
{
    [SerializeField] ParticleSystem coreFx;
    [SerializeField] ParticleSystem RingFx_1;
    [SerializeField] ParticleSystem RingFx_2;
    [SerializeField] ParticleSystem sparklesFx;

    public void BaseJump()
    {
        SetActiveParticles(true, false, false, true);
    }

    public void DoubleJump()
    {
        SetActiveParticles(true, true, false, false);
    }

    public void TripleJump()
    {
        SetActiveParticles(true, true, true, true);
    }

    private void SetActiveParticles(bool coreActive, bool ring1Active, bool ring2Active, bool sparklesActive)
    {
        coreFx.gameObject.SetActive(coreActive);
        RingFx_1.gameObject.SetActive(ring1Active);
        RingFx_2.gameObject.SetActive(ring2Active);
        sparklesFx.gameObject.SetActive(sparklesActive);
    }
}
