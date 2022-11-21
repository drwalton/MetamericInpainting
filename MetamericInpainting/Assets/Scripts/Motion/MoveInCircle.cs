using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInCircle : MonoBehaviour
{
    private float timeCounter = 0.0f;

    public float radius = 4f;
    public float speed = 0.3f;
    public float z = 0.0f;

    public Vector3 initialPosition;

    void Init()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter += Time.deltaTime;
        float angle = speed * timeCounter;
        transform.localPosition =  initialPosition + new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), z);
    }
}
