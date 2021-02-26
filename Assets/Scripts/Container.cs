using System;
using UnityEngine;

namespace Assignment2
{
    /// <summary>
    /// This script handles containers, which are the objects that can be activated (door, safe, chest, etc). Each container can have a lock assigned to it, which 
    /// can be either locked or unlocked. If you click on a container with a locked lock, the lockpicking gameplay mechanism appears.
    /// </summary>
    public class Container : MonoBehaviour
    {
        public static int picksLeft = 20;
        public Transform lockObj; // The lock object that will be activated when clicking on this container

        public bool locked = true; // Is the container locked?

        public GUISkin guiSkin; // A GUI for buttons and labels

        public string lockedCaption = "Locked Door"; // Display text when the lock is locked.
        public string unlockedCaption = "Open Door"; // Display text when the lock is unlocked.

        public float captionWidth = 3000; // GUI caption width.
        public float captionHeight = 1000; // GUI caption height.

        public AudioClip soundLocked; // Locked sound.
        public AudioClip soundActivate; // Activate sound.

        public string lockstitchTag = "Player"; // The tag of the lockstitch. This means that this container can be activated by the object with this tag only
        public string activateButton = "Fire1"; // The button that activates this container

        public ActivateFunctions[] activateFunctions; // A list of functions to be activated. Consists of a reciever and the name of the function to be run in it.
        public FailFunctions[] failFunctions; // A list of functions to be activated on failure. Consists of a reciever and the name of the function to be run in it.


        public string activateAnimation = "DoorOpen"; // Which animation should be played when activating this container. If no animation is set, the default animation is played

        public float detectDistance = 3.0f; // How far off will the lockstitch detect the container from
        public float detectAngle = 180; //

        internal GameObject lockstitch; // The lockstitch object that activates the container.

        internal bool lockstitchDetected = false; // Is the lockstitch detected?
        internal int requiredToolIndex = 0;     // The index of the tool in the inventory

        internal int index = 0; // A general use index
        internal string requiredTool = string.Empty; // The name of the tool in the player's inventory which is required to unlock this lock ( lockpicks, bobbypins, safe cracker tools, etc )

        public float timeLeft = 10;

        public void Start()
        {

            // Assign the lockstitch object by its tag.
            lockstitch = GameObject.FindGameObjectWithTag(lockstitchTag);

            // If there is a lock object assigned
            if (lockObj)
            {
                // Get the name of the required tool from the locks and assign them to requiredTool
                if (lockObj.GetComponent<Lock>())
                {
                    requiredTool = lockObj.GetComponent<Lock>().requiredTool;
                }
               
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public void Update()
        {
            // If an lockstitch has been detected, wait for a button/touch
            if (lockstitchDetected)
            {

                 if (!string.IsNullOrEmpty(activateButton))
                {
                    // If we press the activate button, Activate!
                    if (Input.GetButtonDown(activateButton))
                    {

                        Activate();
                    }
                }
                else
                {
                    // If no activate button is assigned, activate automatically
                    Activate();
                }
            }

        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == lockstitch.tag)
                lockstitchDetected = true;
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.tag == lockstitch.tag)
                lockstitchDetected = false;
        }

        public void ManualActivate()
        {
            lockstitchDetected = true;
        }

        public void AnimateContainer()
        {
            if (GetComponent<Animation>())
            {
                if (!GetComponent<Animation>().isPlaying)
                {
                    // If an activateAnimation is set, play it. Otherwise play the default animation
                    if (activateAnimation != string.Empty)
                    {
                        GetComponent<Animation>().Play("DoorOpen");
                        GetComponent<Animation>().Play(activateAnimation);
                    }
                    else
                    {
                        GetComponent<Animation>().Play();
                    }

                    // Play a sound if avaialble
                    if (soundActivate)
                        GetComponent<AudioSource>().PlayOneShot(soundActivate);
                }
            }
        }

        public void FailActivate()
        {
            // Activate all the fail functions
            foreach (FailFunctions index in failFunctions)
            {
                if (index.reciever && !string.IsNullOrEmpty(index.functionName))
                    index.reciever.SendMessage(index.functionName);
            }
        }

        public void OnGUI()
        {
            GUI.skin = guiSkin;

            if (lockstitchDetected == true)
            {
                if (locked == true)
                {
                    // A caption that displays when the container is locked
                    GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f - captionHeight * 0.5f, captionWidth, captionHeight), lockedCaption);

                    if (CheckRequiredTool())
                    {
                        // A caption that is displayed on a lock that we can open, but only if we have enough lockpicks
                        if (activateButton != string.Empty && lockObj)
                            GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "Press " + activateButton + " to unlock");
                    }
                    else
                    {
                        // A caption that is displayed on a lock that we can open, but we don't have any lockpicks left
                        if (activateButton != string.Empty && lockObj)
                            GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "You don't have " + requiredTool);
                    }
                }
                else
                {
                    // A caption that displays when the container is unlocked
                    GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f - captionHeight * 0.5f, captionWidth, captionHeight), unlockedCaption);

                    // A caption that is displayed on a lock that is unlocked
                    if (activateButton != string.Empty && lockObj)
                        GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "Press " + activateButton + " to open");
                }

            }
        }

        public void Activate()
        {
            // If the container is locked, check if we have a lock assigned to it, in which case the lockpicking gameplay mechanism appears
            if (locked == true)
            {
                // Create a lockpick object and place it in front of the camera, activate lock
                if (lockObj)
                {
                    lockstitchDetected = false;

                    // If the lockstitch is currently active...
                    if (lockstitch.activeSelf)
                    {
                        timeLeft -= Time.deltaTime * 0.4f;
                        // Find the lockstitch component and check if he has any picks left. check how many times left
                        if (CheckRequiredTool() )
                        {

                            // Create a new lock object and place at the center of the screen
                            Transform newLock = (Instantiate(lockObj, lockstitch.transform.position, Quaternion.identity) as Transform);

                            // Check the type of lock component and assign it to the lockstitch accordingly
                            if (newLock.GetComponent<Lock>())
                            {
                                newLock.GetComponent<Lock>().lockParent = transform;
                                newLock.GetComponent<Lock>().lockstitch = lockstitch;
                            }

                            // Deactivate the lockstitch script so it doesn't interfere with the lockpicking gameplay
                            lockstitch.SetActive(false);

                            // Disable this script while we interact with the lock
                            enabled = false;
                        }
                        else if(!CheckRequiredTool())
                        {
                            print("You don't have any " + requiredTool);

                            lockstitchDetected = true;

                            GetComponent<AudioSource>().PlayOneShot(soundLocked);
                        }
                    }
                }

            }
            else
            {
                // If the container is not locked, activate it
                foreach (var index in activateFunctions)
                {
                    if (index.Reciever && index.FunctionName != null)
                        index.Reciever.SendMessage(index.FunctionName);
                }
            }
        }

        /// <summary>
        /// This funtion goes through all the items in the lockstitch's inventory and checks 
        /// if we have at least 1 of the tool required to unlock this container
        /// </summary>
        /// <returns>
        /// Returns whether you have the required tool.
        /// </returns>
        public bool CheckRequiredTool()
        {
            bool returnValue = false;

            if (lockObj != null)
            {
                // Go through all tools in the inventory
                for (int index = 0; index < lockstitch.GetComponent<Inventory>().items.Length; index++)
                {
                    // If we have the correct tool name, and we have at least 1 of it, return true
                    if (lockstitch.GetComponent<Inventory>().items[index].tool == requiredTool && lockstitch.GetComponent<Inventory>().items[index].count > 0)
                    {
                        returnValue = true;

                        // Assign the index of the tool from the inventory to the lock object
                        if (lockObj.GetComponent<Lock>())
                        {
                            Lock.requiredToolIndex = index;
                        }
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Represents a single function to be invoked when you fail.
        /// </summary>
        [Serializable]
        public class FailFunctions
        {
            public Transform reciever;
            public string functionName;
        }

        /// <summary>
        /// Represents a single function to be invoked when the container is activated.
        /// </summary>
        [Serializable]
        public class ActivateFunctions
        {
            public Transform Reciever;
            public string FunctionName;
        }
    }
}