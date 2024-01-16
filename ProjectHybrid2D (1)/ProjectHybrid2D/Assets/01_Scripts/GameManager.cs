using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Camera[] cameras;
    private Display[] displays;

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