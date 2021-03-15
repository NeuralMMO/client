using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this class to an empty game object in your scene and it'll let you take screenshots (meant to be used in Editor)
    /// </summary>
    public class MMScreenshot : MonoBehaviour
    {
        /// the name of the folder (relative to the project's root) to save screenshots to
        public string FolderName = "Screenshots";
        /// the method to use to take the screenshot. Screencapture uses the API of the same name, and will let you keep 
        /// whatever ratio the game view has, RenderTexture renders to a texture of the specified resolution
        public enum Methods { ScreenCapture, RenderTexture }

        [Header("Screenshot")]
        /// the selected method to take a screenshot with. 
        public Methods Method = Methods.ScreenCapture;
        /// the shortcut to watch for to take screenshots
        public KeyCode ScreenshotShortcut = KeyCode.K;

        /// the size by which to multiply the game view when taking the screenshot
        [MMEnumCondition("Method", (int)Methods.ScreenCapture)]        
        public int GameViewSizeMultiplier = 3;

        /// the camera to use to take the screenshot with
        [MMEnumCondition("Method", (int)Methods.RenderTexture)]        
        public Camera TargetCamera;
        /// the width of the desired screenshot
        [MMEnumCondition("Method", (int)Methods.RenderTexture)]
        public int ResolutionWidth;
        /// the height of the desired screenshot
        [MMEnumCondition("Method", (int)Methods.RenderTexture)]
        public int ResolutionHeight;

        [Header("Controls")]
        /// a test button to take screenshots with
        [MMInspectorButton("TakeScreenshot")]
        public bool TakeScreenshotButton;
        
        /// <summary>
        /// At late update, we look for input
        /// </summary>
        protected virtual void LateUpdate()
        {
            DetectInput();
        }

        /// <summary>
        /// If the user presses the screenshot button, we take one
        /// </summary>
        protected virtual void DetectInput()
        {
            if (Input.GetKeyDown(ScreenshotShortcut))
            {
                TakeScreenshot();
            }
        }

        /// <summary>
        /// Takes a screenshot using the specified method and outputs a console log
        /// </summary>
        protected virtual void TakeScreenshot()
        {
            if (!Directory.Exists(FolderName))
            {
                Directory.CreateDirectory(FolderName);
            }

            string savePath = "";
            switch (Method)
            {
                case Methods.ScreenCapture:
                    savePath = TakeScreenCaptureScreenshot();
                    break;

                case Methods.RenderTexture:
                    savePath = TakeRenderTextureScreenshot();
                    break;
            }
            Debug.Log("[MMScreenshot] Screenshot taken and saved at " + savePath);
        }

        /// <summary>
        /// Takes a screenshot using the ScreenCapture API and saves it to file
        /// </summary>
        /// <returns></returns>
        protected virtual string TakeScreenCaptureScreenshot()
        {
            float width = Screen.width * GameViewSizeMultiplier;
            float height = Screen.height * GameViewSizeMultiplier;
            string savePath = FolderName+"/screenshot_" + width + "x" + height + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            ScreenCapture.CaptureScreenshot(savePath, GameViewSizeMultiplier);
            return savePath;
        }

        /// <summary>
        /// Takes a screenshot using a render texture and saves it to file
        /// </summary>
        /// <returns></returns>
        protected virtual string TakeRenderTextureScreenshot()
        {
            string savePath = FolderName + "/screenshot_" + ResolutionWidth + "x" + ResolutionHeight + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            RenderTexture renderTexture = new RenderTexture(ResolutionWidth, ResolutionHeight, 24);
            TargetCamera.targetTexture = renderTexture;
            Texture2D screenShot = new Texture2D(ResolutionWidth, ResolutionHeight, TextureFormat.RGB24, false);
            TargetCamera.Render();
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, ResolutionWidth, ResolutionHeight), 0, 0);
            TargetCamera.targetTexture = null;
            RenderTexture.active = null; 
            Destroy(renderTexture);
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(savePath, bytes);

            return savePath;
        }
    }
}
