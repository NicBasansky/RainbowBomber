using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Dissolver : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private float t = 0.0f;
    private bool dissolve = false;

    public float speed = .5f;
    [SerializeField] float colliderDelay = .5f;
    [SerializeField] float goDelay = 1f;

    private void Start()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();

        //Dissolve();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "BombFX")
    //    {
    //        Dissolve();
    //    }
    //}
   

    public void Dissolve()
    {
        dissolve = true;
        StartCoroutine(DisableObjects());

        // TODO add sound
    }

    IEnumerator DisableObjects()
    {
        yield return new WaitForSeconds(colliderDelay);


        Collider[] colls = GetComponentsInChildren<Collider>();
        foreach (var c in colls)
        {
            c.enabled = false;
        }

        yield return new WaitForSeconds(goDelay);

        Destroy(gameObject);
    }

    private void Update()
    {
        if (dissolve)
        {
            Material[] mats = meshRenderer.materials;

            mats[0].SetFloat("_Cutoff", Mathf.Sin(t * speed));
            t += Time.deltaTime;

            // Unity does not allow meshRenderer.materials[0]...
            meshRenderer.materials = mats;
        }
        
    }
}

