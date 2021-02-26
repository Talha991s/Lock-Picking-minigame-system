using System;
using UnityEngine;

public class Container : MonoBehaviour
{
    public static int pickleft = 20;
    public Transform LockObject;

    public bool locked = true;

    public GUISkin guiSkin;

    public string lockedCaption = "Locked Door";
    public string unlockedCaption = "Opened Door";

    public float captionWidth = 3000;
    public float captionHeight = 1000;

    public AudioClip soundLocked;
    public AudioClip soundActivate;

    public string lockstitchTag = "Player";
    public string activateButton = "Fire1";

    public FailFunctions[] failFunctions;
    public ActivateFunctions[] activateFunctions;

    public string activateAnimation = "Take 001";

    public float detectDistance = 3.0f;
    public float detectAngle = 180;

    internal GameObject lockstitch;

    internal bool lockstitchDetected = false;
    internal int requiredToolIndex = 0;

    internal int index = 0;
    internal string requiredTool = string.Empty;

    public float timeLeft = 10;

    // Start is called before the first frame update
    public void Start()
    {
        lockstitch = GameObject.FindGameObjectWithTag(lockstitchTag);
        if(LockObject)
        {
            if(LockObject.GetComponent<DoorLock>())
            {
                requiredTool = LockObject.GetComponent<DoorLock>().requiredTool;
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
                    if (activateButton != string.Empty && LockObject)
                        GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "Press " + activateButton + " to unlock");
                }
                else
                {
                    // A caption that is displayed on a lock that we can open, but we don't have any lockpicks left
                    if (activateButton != string.Empty && LockObject)
                        GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "You don't have " + requiredTool);
                }
            }
            else
            {
                // A caption that displays when the container is unlocked
                GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f - captionHeight * 0.5f, captionWidth, captionHeight), unlockedCaption);

                // A caption that is displayed on a lock that is unlocked
                if (activateButton != string.Empty && LockObject)
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
            if (LockObject)
            {
                lockstitchDetected = false;

                // If the lockstitch is currently active...
                if (lockstitch.activeSelf)
                {
                    timeLeft -= Time.deltaTime * 0.4f;
                    // Find the lockstitch component and check if he has any picks left. check how many times left
                    if (CheckRequiredTool())
                    {

                        // Create a new lock object and place at the center of the screen
                        Transform newLock = (Instantiate(LockObject, lockstitch.transform.position, Quaternion.identity) as Transform) ;

                        // Check the type of lock component and assign it to the lockstitch accordingly
                        if (newLock.GetComponent<DoorLock>())
                        {
                            newLock.GetComponent<DoorLock>().lockParent = transform;
                            newLock.GetComponent<DoorLock>().lockstitch = lockstitch;
                        }

                        // Deactivate the lockstitch script so it doesn't interfere with the lockpicking gameplay
                        lockstitch.SetActive(false);

                        // Disable this script while we interact with the lock
                        enabled = false;
                    }
                    else if (!CheckRequiredTool())
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

        if (LockObject != null)
        {
            // Go through all tools in the inventory
            for (int index = 0; index < lockstitch.GetComponent<Inventory>().items.Length; index++)
            {
                // If we have the correct tool name, and we have at least 1 of it, return true
                if (lockstitch.GetComponent<Inventory>().items[index].tool == requiredTool && lockstitch.GetComponent<Inventory>().items[index].count > 0)
                {
                    returnValue = true;

                    // Assign the index of the tool from the inventory to the lock object
                    if (LockObject.GetComponent<DoorLock>())
                    {
                        DoorLock.requiredToolIndex = index;
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

