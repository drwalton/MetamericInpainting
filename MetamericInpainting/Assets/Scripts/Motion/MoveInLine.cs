using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInLine : MonoBehaviour
{
    public float speed = 0.3f;
    public Vector3 direction = new Vector3(1, 0, 0);
    Vector3 initialPos;
    public float motionTime = 1.0f;
    public float elapsedTime = 0.0f;


    private void Start()
    {
        initialPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        float displace = elapsedTime % motionTime;
        displace = (elapsedTime % (motionTime * 2)) < motionTime ? displace : motionTime-displace;

        transform.position = initialPos + speed * displace * direction;
        
    }
}
