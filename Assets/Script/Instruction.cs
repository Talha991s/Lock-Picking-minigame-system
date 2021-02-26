using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instruction : MonoBehaviour
{
    public GUISkin guiSkin; //button
    private Rect labelRect = new Rect(0, Screen.height - 40, Screen.width, 40);

    public void OnGUI()
    {
        GUI.skin = guiSkin;

        GUI.Label(labelRect, "Press A/D to Move left/Right. Click the Left Mouse Button to iteract with Objects");
    }
}
