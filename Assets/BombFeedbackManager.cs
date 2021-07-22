using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class BombFeedbackManager : MonoBehaviour
{
    [SerializeField] MMFeedbacks explosionFeedback;


    public void PlayExplosionFeedback()
    {
        explosionFeedback?.PlayFeedbacks();
    }
}
