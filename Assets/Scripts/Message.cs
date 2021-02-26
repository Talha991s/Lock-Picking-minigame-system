using UnityEngine;
using System.Collections;

namespace Assignment2
{
    public class Message : MonoBehaviour
    {
        public GUISkin guiSkin; // GUI for button graphic.

        private Rect labelRect = new Rect(0, Screen.height - 40, Screen.width, 40); // Caching rect of GUI Label Rect, so we don't do it each OnGUI Call.

        public void OnGUI()
        {
            GUI.skin = guiSkin;

            // Some explanation of how to play
            GUI.Label(labelRect, "Press A/D to move left/right. Click the Left Mouse Button to interact with objects");
        }
    }
}