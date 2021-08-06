using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NicLib.SceneManagement
{
    // be sure to uncheck Raycast Target on the UI element that fades
    public class Fader : MonoBehaviour
    {
        CanvasGroup canvasGroup;
        Coroutine currentActiveFade = null;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public Coroutine FadeToBlack(float time)
        {
            return Fade(1, time);
        }

        public Coroutine FadeToClear(float time)
        {
            return Fade(0, time);
        }

        private Coroutine Fade(float alphaTarget, float time)
        {
            if (currentActiveFade != null)
            {
                StopCoroutine(currentActiveFade);
            }
            currentActiveFade = StartCoroutine(FadeRoutine(alphaTarget, time));
            return currentActiveFade;
        }

        public void FadeToBlackImmediate()
        {
            canvasGroup.alpha = 1;
        }

        private IEnumerator FadeRoutine(float alphaTarget, float time)
        {
            while (!Mathf.Approximately(canvasGroup.alpha, alphaTarget))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, alphaTarget, Time.deltaTime / time);
                yield return null;
            }
        }

    }

}
