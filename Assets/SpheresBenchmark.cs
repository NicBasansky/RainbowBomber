using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpheresBenchmark : MonoBehaviour
{
    public int numberOfSpheres = 100;
    Transform[] spheres;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfSpheres; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Specular"));
            rend.material.color = Color.red;
            sphere.transform.position = Random.insideUnitSphere * 20;
        }
        spheres = GameObject.FindObjectsOfType<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Transform[] spheres = GameObject.FindObjectsOfType<Transform>();
            foreach (Transform t in spheres)
            {
                t.Translate(0, 0, 0.01f);
            }
        }
    }
}
