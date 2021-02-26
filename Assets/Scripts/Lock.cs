using System.Collections;
using UnityEngine;

namespace Assignment2
{
    public class Lock : MonoBehaviour
    {
        public static int requiredToolIndex; // The index of the tool in the player's inventory

        public Transform lockpick; // The lockpick object

        public float lockpickHealth = 100; // The health of a fresh new lockpick
        public float breakSpeed = 1; // How quickly a lockpick breaks when stuck

        public float rotateRange = 90; // The rotate range for the lockpick. It's set to 90 meaning that it can go clockwise 90 degress and counterclockwise 90 degrees.

        public float sweetspot = 0;  // The sweetspot center.
        public float sweetspotRange = 20; // The sweetspot range, in which a lock will always open.
        public float sweetspotFalloff = 10; // The sweetspot falloff gradient between the sweetspot range and the break range. This allows the lock cylinder to only partially turn based on how close the lockpick is to the sweetspot range

        public float unlockSpeed = 1; // How quickly the lock cylinder turns.

        public float delayAfterBreak = 1; // How many seconds to wait before replacing a broken lockpick with another.	

        public float jitter = 1; // How violently a lockpick jitters when stuck.

        public bool randomSweetspot = true; // Should the sweetspot center be random?

        // The name of the tool in the player's inventory which is required to unlock this lock ( lockpicks, bobbypins, safe cracker tools, etc )
        public string requiredTool = "lockpicks";
        public AudioClip soundTurn;   // lockstitch turning sound.
        public AudioClip soundStuck;  // lockstitch Stuck sound.
        public AudioClip soundBreak;  // lockstitchpick Breaking sound.
        public AudioClip soundUnlock; // Unlock sound.

        public GUISkin guiSkin; // GUI for button graphics.

        // The position and size of the "abort" button.
        public Rect exitRect = new Rect(700, 0, 100, 50);
        public string exitText = "Exit";

        // The position and size of the "health" display.
        public Rect pickHealthRect = new Rect(700, 50, 100, 80);
        public string pickHealthText = "lockstitcherPick Health: ";

        // The position and size of the "picks left" display.
        public Rect picksLeftRect = new Rect(700, 130, 100, 80);
        public string picksLeftText = "lockstitcherPick Left: ";

        // Time left
        public Rect timeRect = new Rect(700, 210, 100, 50);
        public string timeText = "Time Left: ";

        internal float lockpickHealthCount;
        internal float rotateTarget = 0; // The rotate range target.

        internal Transform lockParent; // The container of this lock ( A door, a safe, etc )
        internal GameObject lockstitch; // The lockstitch object

        internal int lockState = 0; // State of the lock: 0 intro animation, 1 unlocking, 2 picklock broken , 3 unlocked

        // Touch state and index for the lockpick
        internal bool touchlockstitchpick = false;
        internal int touchlockstitchpickIndex = -1;

        // Touch state and index for the Unlock function
        internal bool touchUnlock = false;
        internal int touchUnlockIndex = -1;
        private int angleFix = 360; // A fix for the angle when rotating in minus values.
        private float unlockProgress = 0; // The current progress of the cylinder turning.

        private float timeLeft = 10.0f;
        public void Start()
        {


            //Deactivate the lockstitch script so it doesn't interfere with the lockpicking gameplay

            lockstitch.SetActive(false); // Deactivate the lockstitch script so it desn't interfere with the lockpicking gameplay.

            lockpickHealthCount = lockpickHealth; // Setting the health of a lockpick.

            // If we have a random sweetspot center, 
            if (randomSweetspot == true)
            {
                // Set the sweetspot randomly within the available range. This range must never be larger than the values of sweetspotRange + sweetspotFalloff
                if (rotateRange - sweetspotRange - sweetspotFalloff > 0)
                {
                    sweetspot = Random.Range(-1.0f, 1.0f) * (rotateRange - sweetspotRange - sweetspotFalloff);
                }
                else
                {
                    Debug.LogWarning("Rotate range must not be larger than Sweet Spot Range + Sweet Spot Falloff");
                }
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        public void Update()
        {
            if (lockState == 1 && lockpick.GetComponent<Animation>().isPlaying == false)
            {
                // Check if we are touching the screen, trying to rotate the lock cylinder
                if (touchUnlockIndex > -1)
                {
                    // If there is 1 touch on the screen, assume it belongs to this unlock control. If there are 2 touches, move the control based on the correct index
                    if (Input.touches.Length == 1)
                    {
                        // If the touch has ended or was interrupted reset the index. Otherwise, if the touch is not moving, try to rotate the cylinder
                        if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
                        {
                            touchUnlockIndex = -1;
                        }
                        else if (Input.touches[0].phase != TouchPhase.Moved)
                        {
                            RotateCylinder();
                        }
                    }
                    else if (Input.touches.Length == 2)
                    {
                        // If the touch has ended or was interrupted reset the index. Otherwise, if the touch is not moving, try to rotate the cylinder
                        if (Input.touches[touchUnlockIndex].phase == TouchPhase.Ended || Input.touches[touchUnlockIndex].phase == TouchPhase.Canceled)
                        {
                            touchUnlockIndex = -1;
                        }
                        else if (Input.touches[touchUnlockIndex].phase != TouchPhase.Moved)
                        {
                            RotateCylinder();
                        }
                    }
                }
                else if (Input.GetAxis("Horizontal") != 0)
                {
                    // Check if we are pressing A or D, and try to rotate the lock cylinder
                    RotateCylinder();
                }
                else if (lockState != 3)
                {
                    // Check if we are touching the screen, rotating the lockpick
                    if (touchlockstitchpickIndex > -1)
                    {
                        // If there is 1 touch on the screen, assume it belongs to this lockpick control. If there are 2 touches, move the control based on the correct index
                        if (Input.touches.Length == 1)
                        {
                            // If the touch is moving, rotate the lockpick
                            if (Input.touches[0].phase == TouchPhase.Moved)
                            {
                                // Set the rotate target based on the touch position relative to the screen center (horizontally)
                                rotateTarget = rotateRange * (Input.touches[0].position.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
                            }
                        }
                        else if (Input.touches.Length == 2)
                        {
                            // If the touch index is not reset, proceed
                            if (touchlockstitchpickIndex > -1)
                            {
                                // If the touch is moving, rotate the lockpick
                                if (Input.touches[touchlockstitchpickIndex].phase == TouchPhase.Moved)
                                {
                                    // Set the rotate target based on the mouse position relative to the screen center (horizontally)
                                    rotateTarget = rotateRange * (Input.touches[touchlockstitchpickIndex].position.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Set the rotate target based on the mouse position relative to the screen center (horizontally)
                        rotateTarget = rotateRange * (Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
                    }

                    // Rotate the lockpick towards the rotate target
                    lockpick.localEulerAngles = new Vector3(lockpick.localEulerAngles.x, lockpick.localEulerAngles.y, Mathf.Clamp(rotateTarget, -rotateRange + 0.1f, rotateRange - 0.1f));

                    // Advance the unlock progress
                    unlockProgress -= unlockSpeed * Time.deltaTime;

                    // Stop any sound after it ends. This prevents the cylinder turning sound from looping indefinitely
                    if (GetComponent<AudioSource>().isPlaying == true)
                        GetComponent<AudioSource>().Stop();
                }
                timeLeft -= Time.deltaTime*0.5f;

                if(timeLeft <= 0)
                {
                    Exit();
                }
            }

            // Limit the unlock progress to the length of the unlock animation
            unlockProgress = Mathf.Clamp(unlockProgress, 0, GetComponent<Animation>()["Unlock"].length);

            // If the intro animation ends, move on to the lockpicking stage
            if (GetComponent<Animation>().IsPlaying("Intro") == false && lockState == 0)
            {
                GetComponent<Animation>().Play("Unlock");

                // Set lock state to "unlocking"
                lockState = 1;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Set the time of the animation based on the unlock progress
            GetComponent<Animation>()["Unlock"].time = unlockProgress;

        }

        /// <summary>
        /// Tries to rotate the cylinder. It may get stuck or rotate correctly depending on whether we are in the sweetspot or not.
        /// </summary>
        public void RotateCylinder()
        {
            // Set the angle fix based on the location of the lockpick
            if (lockpick.localEulerAngles.z < 180)
            {
                angleFix = 0;
            }
            else
            {
                angleFix = 360;
            }

            if (lockpick.localEulerAngles.z - angleFix < sweetspot - sweetspotRange - sweetspotFalloff || lockpick.localEulerAngles.z - angleFix > sweetspot + sweetspotRange + sweetspotFalloff)
            {
                // If the lockpick is entirely outside the sweetspot range, start breaking the lockpick
                BreakProgress();
            }
            else if (lockpick.localEulerAngles.z - angleFix < sweetspot - sweetspotRange || lockpick.localEulerAngles.z - angleFix > sweetspot + sweetspotRange)
            {
                // If the lockpick is between the sweetspotFalloff and sweetspotRange values, allow the lock cylinder to turn partially based on how close it is to the sweetspot range
                if (lockpick.localEulerAngles.z - angleFix < sweetspot - sweetspotRange)
                {
                    // Left side sweetspot falloff
                    // Allow the lock cylinder to turn to an extent based on how close we are to the start of the sweetspot range, but then start breaking it
                    // print("progress limit " + (1 - (angleFix + sweetspot - sweetspotRange - lockpick.localEulerAngles.z)/sweetspotFalloff));
                    if (GetComponent<Animation>()["Unlock"].time / GetComponent<Animation>()["Unlock"].length < 1 - (angleFix + sweetspot - sweetspotRange - lockpick.localEulerAngles.z) / sweetspotFalloff)
                    {
                        UnlockProgress();
                    }
                    else
                    {
                        BreakProgress();
                    }
                }

                if (lockpick.localEulerAngles.z - angleFix > sweetspot + sweetspotRange)
                {
                    // Right side sweetspot falloff
                    // Allow the lock cylinder to turn to an extent based on how close we are to the start of the sweetspot range, but then start breaking it
                    // print("progress limit " + ((angleFix + sweetspot + sweetspotRange + sweetspotFalloff - lockpick.localEulerAngles.z)/sweetspotFalloff));
                    if (GetComponent<Animation>()["Unlock"].time / GetComponent<Animation>()["Unlock"].length < (angleFix + sweetspot + sweetspotRange + sweetspotFalloff - lockpick.localEulerAngles.z) / sweetspotFalloff)
                    {
                        UnlockProgress();
                    }
                    else
                    {
                        BreakProgress();
                    }
                }
            }
            else
            {
                // Otherwise, if we are within the sweetspot range, start unlocking the lock
                UnlockProgress();
            }
        }

        /// <summary>
        /// This function goes through the process of breaking a lock. 
        /// It makes the lockpick jitter while reducing its health, and then breaks it.
        /// </summary>
        public void BreakProgress()
        {
            // Set and play relevant sound
            GetComponent<AudioSource>().clip = soundStuck;

            if (GetComponent<AudioSource>().isPlaying == false)
                GetComponent<AudioSource>().Play();

            // Reduce from the health of the lockpick
            lockpickHealthCount -= Time.deltaTime * breakSpeed;

            // Make the lockpick jitter while stuck
            unlockProgress += Random.Range(-unlockSpeed, unlockSpeed) * Time.deltaTime;

            lockpick.localEulerAngles = new Vector3(Random.Range(-jitter, jitter), Random.Range(-jitter, jitter), lockpick.localEulerAngles.z);

            // If the lockpick's health reaches 0, break it
            if (lockpickHealthCount <= 0)
            {
                lockpickHealthCount = 0;

                // Set lock state to "break".
                lockState = 2;

                StartCoroutine(Breaklockstitchpick());
            }
        }

        /// <summary>
        /// This function breaks a lock, throwing it off the lock and removing it.
        /// </summary>
        /// <returns>
        /// The lockpick.
        /// </returns>
        public IEnumerator Breaklockstitchpick()
        {
            GetComponent<AudioSource>().Stop();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            //Create a new lockpick object, give it gravity and throw it away from the lock
            GameObject newlockstitchpick = (Instantiate(lockpick, lockpick.position, lockpick.rotation) as Transform).gameObject;
            newlockstitchpick.GetComponent<Rigidbody>().useGravity = true;
            newlockstitchpick.GetComponent<Rigidbody>().AddForce(Vector3.up * 50);
            newlockstitchpick.GetComponent<Rigidbody>().AddForce(Vector3.forward * 20);
            newlockstitchpick.GetComponent<Rigidbody>().AddForce(Vector3.right * -rotateTarget);
            newlockstitchpick.GetComponent<Rigidbody>().AddTorque(Vector3.forward * rotateTarget * 5);

            //Stop the lockpick animation
            newlockstitchpick.GetComponent<Animation>().Stop();

            // Destroy the new lockpick after 3 seconds
            Destroy(newlockstitchpick.gameObject, 3);

            // Deactivate the lockpick
            lockpick.gameObject.SetActive(false);

            // Play break sound.
            GetComponent<AudioSource>().PlayOneShot(soundBreak);

            // Wait for some time, and then...
            yield return new WaitForSeconds(delayAfterBreak);

            //Activate the mouse cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // If we have a limited number of lockpicks, take them into consideration. Otherwise, we have infinite lockpicks
            if (lockstitch.GetComponent<Inventory>().items[requiredToolIndex].count > 0)
            {
                // Reduce from the number of lockpicks the lockstitch has
                lockstitch.GetComponent<Inventory>().items[requiredToolIndex].count -= 1;

                print(lockstitch.GetComponent<Inventory>().items[requiredToolIndex].tool);

                if (lockstitch.GetComponent<Inventory>().items[requiredToolIndex].count <= 0)
                {
                    Destroy(lockpick.gameObject);

                    Exit();

                    yield return true;
                }
            }

            // Reactivate the lockpick.
            lockpick.gameObject.SetActive(true);

            // Reset the lockpick health count.
            lockpickHealthCount = lockpickHealth;

            // Set lock state to "unlocking".
            lockState = 1;

            // Reset touch states
            touchlockstitchpickIndex = -1;
            touchUnlockIndex = -1;
        }

        /// <summary>
        /// Goes through the process of unlocking, turning the lock until it unlocks completely.
        /// </summary>
        public void UnlockProgress()
        {
            // Set and play relevant sound
            GetComponent<AudioSource>().clip = soundTurn;

            if (GetComponent<AudioSource>().isPlaying == false)
                GetComponent<AudioSource>().Play();

            // Advance the unlock progress
            unlockProgress += unlockSpeed * Time.deltaTime;

            // If we reach the end of the unlock animation, we WIN
            if (GetComponent<Animation>().isPlaying == false)
            {
                lockState = 3; // Set lock state to "unlocked"

                StartCoroutine(Unlock());
            }
        }

        /// <summary>
        /// Unlocks and opens a container. 
        /// After that the container will have an unlocked lock.
        /// </summary>
        public IEnumerator Unlock()
        {
            // Set and play relevant sounds
            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().PlayOneShot(soundUnlock);

            // Wait for a second
            yield return new WaitForSeconds(1);

            Container container = lockParent.GetComponent<Container>();

            // Set the container to unlocked and activate it
            container.locked = false;
            container.Activate();

            Exit();
        }

        /// <summary>
        /// Aborts the lockpicking gameplay and reactivates the lockstitch.
        /// </summary>
        public void Exit()
        {
            // Activate the lockstitch prefab, meaning that we are done with lockpicking
            lockstitch.SetActive(true);

            // Enable the container script
            lockParent.GetComponent<Container>().enabled = true;

            // Destroy this lock
            Destroy(gameObject);
        }
        public void CheckTouch()
        {
            // Look for 2 touches
            if (Input.touchCount <= 2)
            {
                // Go through all available touches
                for (int index = 0; index < Input.touchCount; index++)
                {
                    // If the touch just began
                    if (Input.touches[index].phase == TouchPhase.Began)
                    {
                        // If the touch is within the top half of the screen, assign it to the lockpick
                        if (Input.touches[index].position.y > Screen.height * 0.5f)
                        {
                            touchlockstitchpick = true;
                            touchlockstitchpickIndex = index;
                        }

                        // If the touch is within the bottom half of the screen, assign it to the unlock
                        if (Input.touches[index].position.y < Screen.height * 0.5f)
                        {
                            touchUnlock = true;
                            touchUnlockIndex = index;
                        }
                    }

                    // If a touch has ended, reset its index
                    if (Input.touches[index].phase == TouchPhase.Ended)
                    {
                        // Reset index for lockpick
                        if (index == touchlockstitchpickIndex)
                        {
                            touchlockstitchpick = false;
                            touchlockstitchpickIndex = -1;
                        }

                        // Reset index for unlock
                        if (index == touchUnlockIndex)
                        {
                            touchUnlock = false;
                            touchUnlockIndex = -1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows you to draw on screen in editor Gizmos for objects.
        /// </summary>
        public void OnDrawGizmos()
        {
            // The right edge of the of the sweetspot range
            var angle = Quaternion.AngleAxis(sweetspot + sweetspotRange, transform.forward) * transform.up;
            Debug.DrawLine(transform.position, transform.position + angle, Color.green);

            // The left edge of the of the sweetspot range
            angle = Quaternion.AngleAxis(sweetspot - sweetspotRange, transform.forward) * transform.up;
            Debug.DrawLine(transform.position, transform.position + angle, Color.green);

            // The right edge of the of the sweetspot falloff range
            angle = Quaternion.AngleAxis(sweetspot + sweetspotRange + sweetspotFalloff, transform.forward) * transform.up;
            Debug.DrawLine(transform.position, transform.position + angle, Color.red);

            // The left edge of the of the sweetspot falloff range
            angle = Quaternion.AngleAxis(sweetspot - sweetspotRange - sweetspotFalloff, transform.forward) * transform.up;
            Debug.DrawLine(transform.position, transform.position + angle, Color.red);
        }

        public void OnGUI()
        {
            GUI.skin = guiSkin;

            // Exit button
            if (GUI.Button(exitRect, exitText))
                Exit();
            // Display lockpick health
            GUI.Label(pickHealthRect, pickHealthText + "\n" + (Mathf.Round((lockpickHealthCount / lockpickHealth) * 80)).ToString());

            // Display lockpicks left
            GUI.Label(picksLeftRect, picksLeftText + lockstitch.GetComponent<Inventory>().items[requiredToolIndex].count);

            GUI.Label(timeRect, timeText + Mathf.Round(timeLeft));

            GUI.Label(new Rect(0, Screen.height - 60, Screen.width, 60), "Use the mouse to rotate the pick, and then press A or D to turn the lock cylinder.\nTry to find the sweetspot for the pick or the pick will break.");

        }

    }
}