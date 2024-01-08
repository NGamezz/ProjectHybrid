using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraObject : MonoBehaviour
{
    [SerializeField] private Canvas[] Canvas;
    [SerializeField] public Camera camera;
    public bool IsMainCamera = false;

    public void SetDisplay ( RenderBuffer renderBufferA, RenderBuffer renderBufferB )
    {
        camera.SetTargetBuffers(renderBufferA, renderBufferB);
        camera.depth = Camera.main.depth - 1;

        foreach ( var canv in Canvas )
        {
            canv.targetDisplay = camera.targetDisplay;
        }
    }

    private void Awake ()
    {
        Canvas = GetComponentsInChildren<Canvas>();
        camera = GetComponent<Camera>();
    }
}