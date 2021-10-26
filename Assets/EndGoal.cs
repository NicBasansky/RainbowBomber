using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bomber.Control;
using UnityEngine.SceneManagement;
using NicLib.SceneManagement;

public class EndGoal : MonoBehaviour
{
    [SerializeField] GameObject[] endGoalGameObjects;
    [SerializeField] float sceneLoadDelay = 3.0f;
    [SerializeField] Fader fader;

    private void Start()
    {
        fader.FadeToClear(0.5f);
        
    }

    // called by the quest giver
    public void MakeGoalVisible(bool visible)
    {
        foreach(GameObject go in endGoalGameObjects)
        {
            go.SetActive(visible);
        }
    }

    public void GoalReached(Collider other)
    {
   
        // disable controls and movement
        var pc = other.GetComponent<PlayerController>();
        pc.FreezeMovement();

        // lerp to center
        pc.LerpToPoint(transform.position + new Vector3(0, 0.5f, 0));

        // play sound

        // save points
       
        StartCoroutine(LoadNextScene());          
    }

    IEnumerator LoadNextScene()
    {
        fader.FadeToBlack(3.0f);

        yield return new WaitForSeconds(sceneLoadDelay);

        // TODO figure out what to do with the last scene
        int sceneIndex = (SceneManager.GetActiveScene().buildIndex + 1);// % SceneManager.sceneCount;
        SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);

    }

}
