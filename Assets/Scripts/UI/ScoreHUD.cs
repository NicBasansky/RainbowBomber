using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace Bomber.UI
{
    public class ScoreHUD : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI scoreCount;
        int score = 0;

        // Start is called before the first frame update
        void Start()
        {
            scoreCount.text = score.ToString();
        }

        public void UpdateScore(int delta)
        {
            score += delta;
            scoreCount.text = score.ToString();
        }
    }
}
