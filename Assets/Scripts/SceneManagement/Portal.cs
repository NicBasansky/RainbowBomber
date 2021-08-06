using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
//using RPG.Core;
//using RPG.Control;

// Requires the player have a PlayerController script
// Requires the saving system if you want to save anything when changing scenes
namespace NicLib.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        public enum DestinationIdentifier
        {
            A,
            B,
            C,
            D
        }

        [SerializeField] int sceneToLoadInBuildIndex = -1;
        [SerializeField] DestinationIdentifier destinationIdentifier;
        [SerializeField] Transform spawnLocation;
        [SerializeField] float fadeToBlackSeconds = 1.0f;
        [SerializeField] float fadeToClearSeconds = 1.0f;
        [SerializeField] float fadeWaitTime = 0.5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                if (sceneToLoadInBuildIndex > -1)
                {
                    StartCoroutine(Transition());
                }
                else
                {
                    print(gameObject.name + " needs to have to sceneToLoad variable set");
                }
            }
        }

        private IEnumerator Transition()
        {
            DontDestroyOnLoad(this.gameObject);

            Fader fader = FindObjectOfType<Fader>();

            // disable player control
            //GameObject.FindWithTag("Player").GetComponent<PlayerController>().enabled = false;

            yield return fader.FadeToBlack(fadeToBlackSeconds);

            //FindObjectOfType<SavingWrapper>().Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoadInBuildIndex);

            //PlayerController pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            //pc.enabled = false;

            //FindObjectOfType<SavingWrapper>().Load();

            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);

            //FindObjectOfType<SavingWrapper>().Save();

            yield return new WaitForSeconds(fadeWaitTime);

            fader.FadeToClear(fadeToClearSeconds);

            //GameObject.FindWithTag("Player").GetComponent<PlayerController>().enabled = true;

            Destroy(gameObject);
        }

        private Portal GetOtherPortal()
        {
            Portal[] portals = FindObjectsOfType<Portal>();
            foreach (Portal portal in portals)
            {
                if (portal == this) continue;
                if (this.destinationIdentifier != portal.destinationIdentifier) continue;

                return portal;
            }
            return null;
        }

        // todo see if player placement is incorrect, may need to disable/re-enable nav mesh agent
        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnLocation.position);
            player.transform.rotation = otherPortal.spawnLocation.rotation;
        }


    }

}