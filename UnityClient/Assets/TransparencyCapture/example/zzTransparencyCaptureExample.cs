using System.Collections;
using UnityEngine;

public class zzTransparencyCaptureExample:MonoBehaviour
{
    public Texture2D capturedImage;
    public Transform cameraTransform;

    void Start()
    {
        lastMousePosition = Input.mousePosition;
    }

    public IEnumerator capture()
    {
        //capture whole screen
        Rect lRect = new Rect(0f,0f,Screen.width,Screen.height);
        if(capturedImage)
            Destroy(capturedImage);

        yield return new WaitForEndOfFrame();
        //After Unity4,you have to do this function after WaitForEndOfFrame in Coroutine
        //Or you will get the error:"ReadPixels was called to read pixels from system frame buffer, while not inside drawing frame"
        capturedImage = zzTransparencyCapture.capture(lRect);
    }

    Vector3 lastMousePosition;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            StartCoroutine(capture());
        if (Input.GetKeyDown(KeyCode.S))
            Destroy(capturedImage);

        //Update camera position
        Vector3 lTranslate = Input.mousePosition - lastMousePosition;
        lTranslate*=0.15f;
        cameraTransform.Translate(lTranslate);
        lastMousePosition = Input.mousePosition;
    }

    void OnGUI()
    {

        if (capturedImage)
        {
            GUI.DrawTexture(
                new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.8f),
                capturedImage,
                ScaleMode.ScaleToFit,
                true);
            GUI.color = Color.green;
            GUILayout.Label("press S to clear");
        }

        GUI.color = Color.black;
        GUILayout.Label("Press C to do transparent capturing, please capture those boxes");
        GUILayout.Label("The result won't include background color, and the transparency (alpha value) in scene objects, is also can be captured");
    }
}