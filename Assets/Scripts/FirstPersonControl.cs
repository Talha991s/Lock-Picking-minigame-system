using UnityEngine;

namespace Assignment2
{
    public class FirstPersonControl : MonoBehaviour
    {
        public float speed = 10; // The player's speed.

        public void FixedUpdate()
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                transform.Translate(-Vector3.right * speed * Input.GetAxis("Horizontal") * Time.deltaTime, Space.World);
            }

        }
    }
}