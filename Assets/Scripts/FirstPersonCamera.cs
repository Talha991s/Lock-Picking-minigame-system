using UnityEngine;
using System.Collections;

namespace Assignment2
{

    public class FirstPersonCamera : MonoBehaviour
    {
        public Transform target; // The object to follow.
        public Vector3 offset; // How far from the object the camera hovers.
        public float followSpeed = 5; // How fast the camera follows the target.
	
        public void Update()
        {
            if (target)
            {
                transform.position = Vector3.Slerp(transform.position, target.position + offset, followSpeed * Time.deltaTime);
            }
        }

    }
}
