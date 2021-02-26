using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float speed = 10.0f;

    public void FixedUpdate()
    {
        if(Input.GetAxis("Horizontal")!=0)
        {
            transform.Translate(Vector3.right * speed * Input.GetAxis("Horizontal") * Time.deltaTime, Space.World);
        }
    }
}
