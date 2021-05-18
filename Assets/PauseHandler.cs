using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bomber.UI
{
    public class PauseHandler : MonoBehaviour
    {
        [SerializeField] PauseUI pauseMenu;
        MasterInputControls controls;
        bool isPaused = false;

        private void Awake()
        {
            controls = new MasterInputControls();

            // The syntax is wierd here. context is var that can be named anything and holds context information
            // that we don't need but still have to write it. 
            controls.Player.Pause.performed += context => Pause();
        }

        private void Start()
        {
            pauseMenu.gameObject.SetActive(false);
        }

        void Pause()
        {
            isPaused = !isPaused;
            
            if(isPaused)
            {
                Time.timeScale = 0;
                pauseMenu.gameObject.SetActive(true);
                
            }
            else
            {
                Time.timeScale = 1;
                pauseMenu.gameObject.SetActive(false);
            }

        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }
    }

}