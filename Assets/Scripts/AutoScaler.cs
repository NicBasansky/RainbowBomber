using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScaler : MonoBehaviour
{
    [SerializeField] Vector3 maxScale = new Vector3(5, 1, 5);
    [SerializeField] float incrAmount = 0.1f;
    [SerializeField] float speed = 1f;

    private void OnEnable()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    public void SetParams(Vector3 maxScale, float incrAmount, float speed)
    {
        this.maxScale = maxScale;
        this.incrAmount = incrAmount;
        this.speed = speed;
    }
    
    void Update()
    {
        float x = transform.localScale.x + incrAmount * speed * Time.deltaTime;
        x = Mathf.Min(x, maxScale.x);
        float y = transform.localScale.y + incrAmount * speed * Time.deltaTime;
        y = Mathf.Min(y, maxScale.y);
        float z = transform.localScale.z + incrAmount * speed * Time.deltaTime;
        z = Mathf.Min(z, maxScale.z);

        transform.localScale = new Vector3(x, y, z);    
    }
}

// Settings: Largest explosion maxScale 2, 1, 2, incrAmount 0.3, speed 5
