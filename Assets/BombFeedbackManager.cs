using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class BombFeedbackManager : Singleton<BombFeedbackManager>
{
    [SerializeField] MMFeedbacks explosionFeedback;


    public void PlayExplosionFeedback()
    {
        explosionFeedback?.PlayFeedbacks();
    }
}
