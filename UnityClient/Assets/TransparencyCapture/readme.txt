Transparency Capture

Capture view with transparency included.

How to use:
Use zzTransparencyCapture.capture(Rect) to capture a rectangular area from view.
zzTransparencyCapture.captureScreenshot capture whole screen.

After Unity4,you have to do those function after WaitForEndOfFrame in Coroutine
Or you will get the error:"ReadPixels was called to read pixels from system frame buffer, while not inside drawing frame"

How does it work:
This is the core. I use Texture2D.ReadPixels to make two captures with black and white background color separately.
Then calculate the alpha value in every pixel, with the two captures.
Thanks for Wang Xiang thinking out the idea, it saved me a lot of time to realize the function.

If you have some advice, some want, or find some bug, you can contact me.

Author:
orange030@gmail.com