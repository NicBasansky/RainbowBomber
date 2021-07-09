using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomber.UI
{
    public class PauseUI : MonoBehaviour
    {
        public void ResumePlay()
        {
            print("Resuming Play");
        }

        public void RestartLevel()
        {
            print("Restarting Level");
        }

        public void Quit()
        {
            print("Quitting application");
        }
    }

}