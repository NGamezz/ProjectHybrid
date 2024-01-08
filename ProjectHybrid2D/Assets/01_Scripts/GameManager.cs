using System.ComponentModel;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //[SerializeField] private CameraObject[] cameras;
    private Camera[] cameras;
    private Display[] displays;

    //public Display GetDisplay ()
    //{
    //    Display display;
    //    lock ( displays )
    //    {
    //        display = displays[displayIndex];
    //        displayIndex++;
    //    }
    //    return display;
    //}

    private void Awake ()
    {
        cameras = FindObjectsByType<Camera>(FindObjectsSortMode.InstanceID);
    }

    void Start ()
    {
        displays = Display.displays;
        var mainDisplay = Display.main;
        foreach ( var display in displays )
        {
            display.Activate();
        }

        var displayIndex = 1;
        for ( int i = 0; i < cameras.Length; i++ )
        {
            var currentCam = cameras[i];

            if ( i >= displays.Length )
            {
                currentCam.enabled = false;
                continue;
            }

            if ( currentCam != Camera.main )
            {
                currentCam.enabled = true;
                currentCam.SetTargetBuffers(displays[displayIndex].colorBuffer, displays[displayIndex].depthBuffer);
                displayIndex = (displayIndex + 1) % displays.Length;
            }
            else
            {
                currentCam.enabled = true;
                currentCam.SetTargetBuffers(mainDisplay.colorBuffer, mainDisplay.depthBuffer);
            }
        }
    }
}