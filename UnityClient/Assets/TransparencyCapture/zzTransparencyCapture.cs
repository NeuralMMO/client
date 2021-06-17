using UnityEngine;
using System.IO;

public class zzTransparencyCapture
{
    public static Texture2D  capture(Rect pRect)
    {
        Camera lCamera = Camera.main;
        Texture2D lOut;
        var lPreClearFlags = lCamera.clearFlags;
        var lPreBackgroundColor = lCamera.backgroundColor;
        {
            lCamera.clearFlags = CameraClearFlags.Color;

            //make two captures with black and white background
            lCamera.backgroundColor = Color.black;
            lCamera.Render();
            var lBlackBackgroundCapture = captureView(pRect);

            lCamera.backgroundColor = Color.white;
            lCamera.Render();
            var lWhiteBackgroundCapture = captureView(pRect);

            for (int x = 0; x < lWhiteBackgroundCapture.width; ++x)
            {
                for (int y = 0; y < lWhiteBackgroundCapture.height; ++y)
                {
                    Color lColorWhenBlack = lBlackBackgroundCapture.GetPixel(x, y);
                    Color lColorWhenWhite = lWhiteBackgroundCapture.GetPixel(x, y);
                    if (lColorWhenBlack != Color.clear)
                    {
                        //set real color
                        lWhiteBackgroundCapture.SetPixel(x, y,
                            getColor(lColorWhenBlack, lColorWhenWhite));
                    }
                }
            }
            lWhiteBackgroundCapture.Apply();
            lOut = lWhiteBackgroundCapture;
            Object.DestroyImmediate(lBlackBackgroundCapture);
        }
        lCamera.backgroundColor = lPreBackgroundColor;
        lCamera.clearFlags = lPreClearFlags;
        return lOut;
    }

    /// <summary>
    /// Capture a screenshot(not include GUI)
    /// </summary>
    /// <returns></returns>
    public static Texture2D captureScreenshot()
    {
        return capture(new Rect(0f, 0f, Screen.width, Screen.height));
    }

    /// <summary>
    /// Capture a screenshot(not include GUI) at path filename as a PNG file
    /// eg. zzTransparencyCapture.captureScreenshot("Screenshot.png")
    /// </summary>
    /// <param name="pFileName"></param>
    /// <returns></returns>
    public static void captureScreenshot(string pFileName)
    {
        var lScreenshot = captureScreenshot();
        try
        {
            using (var lFile = new FileStream(pFileName, FileMode.Create))
            {
                BinaryWriter lWriter = new BinaryWriter(lFile);
                lWriter.Write(lScreenshot.EncodeToPNG());
            }
        }
        finally
        {
            Object.DestroyImmediate(lScreenshot);
        }
    }

    //pColorWhenBlack!=Color.clear
    static Color getColor(Color pColorWhenBlack,Color pColorWhenWhite)
    {
        float lAlpha = getAlpha(pColorWhenBlack.r, pColorWhenWhite.r);
        return new Color(
            pColorWhenBlack.r / lAlpha,
            pColorWhenBlack.g / lAlpha,
            pColorWhenBlack.b / lAlpha,
            lAlpha);
    }


    //           Color*Alpha      Color   Color+(1-Color)*(1-Alpha)=1+Color*Alpha-Alpha
    //0----------ColorWhenZero----Color---ColorWhenOne------------1
    static float getAlpha(float pColorWhenZero, float pColorWhenOne)
    {
        //pColorWhenOne-pColorWhenZero=1-Alpha
        return 1 + pColorWhenZero - pColorWhenOne;
    }

    static Texture2D captureView(Rect pRect)
    {
        Texture2D lOut = new Texture2D((int)pRect.width, (int)pRect.height, TextureFormat.ARGB32, false);
        lOut.ReadPixels(pRect, 0, 0, false);
        return lOut;
    }

}