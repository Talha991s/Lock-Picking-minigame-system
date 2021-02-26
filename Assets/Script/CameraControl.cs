using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float followspeed;

    // Update is called once per frame
    public void Update()
    {
        if(target)
        {
            transform.position = Vector3.Slerp(transform.position, target.position + offset, followspeed * Time.deltaTime);
        }
    }
}
