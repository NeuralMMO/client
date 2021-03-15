using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// The formulas described here are (loosely) based on Robert Penner's easing equations http://robertpenner.com/easing/
    /// I recommend reading this blog post if you're interested in the subject : http://blog.moagrius.com/actionscript/jsas-understanding-easing/
    /// </summary>

    public class MMTween : MonoBehaviour
    {
        /// <summary>
        /// A list of all the possible curves you can tween a value along
        /// </summary>
        public enum MMTweenCurve
        {
            LinearTween,        
            EaseInQuadratic,    EaseOutQuadratic,   EaseInOutQuadratic,
            EaseInCubic,        EaseOutCubic,       EaseInOutCubic,
            EaseInQuartic,      EaseOutQuartic,     EaseInOutQuartic,
            EaseInQuintic,      EaseOutQuintic,     EaseInOutQuintic,
            EaseInSinusoidal,   EaseOutSinusoidal,  EaseInOutSinusoidal,
            EaseInBounce,       EaseOutBounce,      EaseInOutBounce,
            EaseInOverhead,     EaseOutOverhead,    EaseInOutOverhead,
            EaseInExponential,  EaseOutExponential, EaseInOutExponential,
            EaseInElastic,      EaseOutElastic,     EaseInOutElastic,
            EaseInCircular,     EaseOutCircular,    EaseInOutCircular,
        }

        public static Coroutine MoveTransform(MonoBehaviour mono, Transform targetTransform, Transform origin, Transform destination, WaitForSeconds delay, float delayDuration, float duration, MMTween.MMTweenCurve curve, bool updatePosition = true, bool updateRotation = true)
        {
            return mono.StartCoroutine(MoveTransformCo(targetTransform, origin, destination, delay, delayDuration, duration, curve, updatePosition, updateRotation));
        }

        protected static IEnumerator MoveTransformCo(Transform targetTransform, Transform origin, Transform destination, WaitForSeconds delay, float delayDuration, float duration, MMTween.MMTweenCurve curve, bool updatePosition = true, bool updateRotation = true)
        {
            if (delayDuration > 0f)
            {
                yield return delay;
            }
            float timeLeft = duration;
            while (timeLeft > 0f)
            {
                if (updatePosition)
                {
                    targetTransform.transform.position = MMTween.Tween(duration - timeLeft, 0f, duration, origin.position, destination.position, curve);
                }
                if (updateRotation)
                {
                    targetTransform.transform.rotation = MMTween.Tween(duration - timeLeft, 0f, duration, origin.rotation, destination.rotation, curve);
                }
                timeLeft -= Time.deltaTime;
                yield return null;
            }
            if (updatePosition) { targetTransform.transform.position = destination.position; }
            if (updateRotation) { targetTransform.transform.localEulerAngles = destination.localEulerAngles; }
        }

        public static Coroutine RotateTransformAround(MonoBehaviour mono, Transform targetTransform, Transform center, Transform destination, float angle, WaitForSeconds delay, float delayDuration, float duration, MMTween.MMTweenCurve curve)
        {
            return mono.StartCoroutine(RotateTransformAroundCo(targetTransform, center, destination, angle, delay, delayDuration, duration, curve));
        }

        protected static IEnumerator RotateTransformAroundCo(Transform targetTransform, Transform center, Transform destination, float angle, WaitForSeconds delay, float delayDuration, float duration, MMTween.MMTweenCurve curve)
        {
            if (delayDuration > 0f)
            {
                yield return delay;
            }

            Vector3 initialRotationPosition = targetTransform.transform.position;
            Quaternion initialRotationRotation = targetTransform.transform.rotation;

            float rate = 1f / duration;

            float timeSpent = 0f;
            while (timeSpent < duration)
            {

                float newAngle = MMTween.Tween(timeSpent, 0f, duration, 0f, angle, curve);

                targetTransform.transform.position = initialRotationPosition;
                initialRotationRotation = targetTransform.transform.rotation;
                targetTransform.RotateAround(center.transform.position, center.transform.up, newAngle);
                targetTransform.transform.rotation = initialRotationRotation;

                timeSpent += Time.deltaTime;
                yield return null;
            }
            targetTransform.transform.position = destination.position;
        }

        public static Vector2 Tween(float currentTime, float initialTime, float endTime, Vector2 startValue, Vector2 endValue, MMTweenCurve curve)
        {
            startValue.x = Tween(currentTime, initialTime, endTime, startValue.x, endValue.x, curve);
            startValue.y = Tween(currentTime, initialTime, endTime, startValue.y, endValue.y, curve);
            return startValue;
        }

        public static Vector3 Tween(float currentTime, float initialTime, float endTime, Vector3 startValue, Vector3 endValue, MMTweenCurve curve)
        {
            startValue.x = Tween(currentTime, initialTime, endTime, startValue.x, endValue.x, curve);
            startValue.y = Tween(currentTime, initialTime, endTime, startValue.y, endValue.y, curve);
            startValue.z = Tween(currentTime, initialTime, endTime, startValue.z, endValue.z, curve);
            return startValue;
        }

        public static Quaternion Tween(float currentTime, float initialTime, float endTime, Quaternion startValue, Quaternion endValue, MMTweenCurve curve)
        {
            float turningRate = Tween(currentTime, initialTime, endTime, 0f, 1f, curve);
            startValue = Quaternion.Slerp(startValue, endValue, turningRate);
            return startValue;
        }

        /// <summary>
        /// Moves a value between a startValue and an endValue based on a currentTime, along the specified tween curve
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="initialTime"></param>
        /// <param name="endTime"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static float Tween(float currentTime, float initialTime, float endTime, float startValue, float endValue, MMTweenCurve curve)
        {            
            currentTime = MMMaths.Remap(currentTime, initialTime, endTime, 0f, 1f);
            switch (curve)
            {
                case MMTweenCurve.LinearTween: currentTime = MMTweenDefinitions.Linear_Tween(currentTime); break;

                case MMTweenCurve.EaseInQuadratic:      currentTime = MMTweenDefinitions.EaseIn_Quadratic(currentTime); break;
                case MMTweenCurve.EaseOutQuadratic:     currentTime = MMTweenDefinitions.EaseOut_Quadratic(currentTime); break;
                case MMTweenCurve.EaseInOutQuadratic:   currentTime = MMTweenDefinitions.EaseInOut_Quadratic(currentTime); break;

                case MMTweenCurve.EaseInCubic: currentTime = MMTweenDefinitions.EaseIn_Cubic(currentTime); break;
                case MMTweenCurve.EaseOutCubic: currentTime = MMTweenDefinitions.EaseOut_Cubic(currentTime); break;
                case MMTweenCurve.EaseInOutCubic: currentTime = MMTweenDefinitions.EaseInOut_Cubic(currentTime); break;

                case MMTweenCurve.EaseInQuartic: currentTime = MMTweenDefinitions.EaseIn_Quartic(currentTime); break;
                case MMTweenCurve.EaseOutQuartic: currentTime = MMTweenDefinitions.EaseOut_Quartic(currentTime); break;
                case MMTweenCurve.EaseInOutQuartic: currentTime = MMTweenDefinitions.EaseInOut_Quartic(currentTime); break;

                case MMTweenCurve.EaseInQuintic: currentTime = MMTweenDefinitions.EaseIn_Quintic(currentTime); break;
                case MMTweenCurve.EaseOutQuintic: currentTime = MMTweenDefinitions.EaseOut_Quintic(currentTime); break;
                case MMTweenCurve.EaseInOutQuintic: currentTime = MMTweenDefinitions.EaseInOut_Quintic(currentTime); break;

                case MMTweenCurve.EaseInSinusoidal: currentTime = MMTweenDefinitions.EaseIn_Sinusoidal(currentTime); break;
                case MMTweenCurve.EaseOutSinusoidal: currentTime = MMTweenDefinitions.EaseOut_Sinusoidal(currentTime); break;
                case MMTweenCurve.EaseInOutSinusoidal: currentTime = MMTweenDefinitions.EaseInOut_Sinusoidal(currentTime); break;

                case MMTweenCurve.EaseInBounce: currentTime = MMTweenDefinitions.EaseIn_Bounce(currentTime); break;
                case MMTweenCurve.EaseOutBounce: currentTime = MMTweenDefinitions.EaseOut_Bounce(currentTime); break;
                case MMTweenCurve.EaseInOutBounce: currentTime = MMTweenDefinitions.EaseInOut_Bounce(currentTime); break;

                case MMTweenCurve.EaseInOverhead: currentTime = MMTweenDefinitions.EaseIn_Overhead(currentTime); break;
                case MMTweenCurve.EaseOutOverhead: currentTime = MMTweenDefinitions.EaseOut_Overhead(currentTime); break;
                case MMTweenCurve.EaseInOutOverhead: currentTime = MMTweenDefinitions.EaseInOut_Overhead(currentTime); break;

                case MMTweenCurve.EaseInExponential: currentTime = MMTweenDefinitions.EaseIn_Exponential(currentTime); break;
                case MMTweenCurve.EaseOutExponential: currentTime = MMTweenDefinitions.EaseOut_Exponential(currentTime); break;
                case MMTweenCurve.EaseInOutExponential: currentTime = MMTweenDefinitions.EaseInOut_Exponential(currentTime); break;

                case MMTweenCurve.EaseInElastic: currentTime = MMTweenDefinitions.EaseIn_Elastic(currentTime); break;
                case MMTweenCurve.EaseOutElastic: currentTime = MMTweenDefinitions.EaseOut_Elastic(currentTime); break;
                case MMTweenCurve.EaseInOutElastic: currentTime = MMTweenDefinitions.EaseInOut_Elastic(currentTime); break;

                case MMTweenCurve.EaseInCircular: currentTime = MMTweenDefinitions.EaseIn_Circular(currentTime); break;
                case MMTweenCurve.EaseOutCircular: currentTime = MMTweenDefinitions.EaseOut_Circular(currentTime); break;
                case MMTweenCurve.EaseInOutCircular: currentTime = MMTweenDefinitions.EaseInOut_Circular(currentTime); break;

            }
            return startValue + currentTime * (endValue - startValue);
        }
    }

    public class MMTweenDefinitions
    {
        // Linear       ---------------------------------------------------------------------------------------------------------------------------

        public static float Linear_Tween(float t)
        {
            return t;
        }

        public static float LinearAnti_Tween(float t)
        {
            return 1-t;
        }

        // Quadratic    ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Quadratic(float t)
        {
            return t * t;
        }

        public static float EaseOut_Quadratic(float t)
        {
            return 1 - EaseIn_Quadratic(1-t);
        }

        public static float EaseInOut_Quadratic(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Quadratic(t*2f)/2f;
            }
            else
            {
                return 1 - EaseIn_Quadratic((1f-t)*2f)/2;
            }
        }

        // Cubic        ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Cubic(float t)
        {
            return t * t * t;
        }

        public static float EaseOut_Cubic(float t)
        {
            return 1 - EaseIn_Cubic(1 - t);
        }

        public static float EaseInOut_Cubic(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Cubic(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Cubic((1f - t) * 2f) / 2;
            }
        }

        // Quartic      ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Quartic(float t)
        {
            return Mathf.Pow(t, 4f);
        }

        public static float EaseOut_Quartic(float t)
        {
            return 1 - EaseIn_Quartic(1 - t);
        }

        public static float EaseInOut_Quartic(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Quartic(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Quartic((1f - t) * 2f) / 2;
            }
        }

        // Quintic      ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Quintic(float t)
        {
            return Mathf.Pow(t, 5f);
        }

        public static float EaseOut_Quintic(float t)
        {
            return 1 - EaseIn_Quintic(1 - t);
        }

        public static float EaseInOut_Quintic(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Quintic(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Quintic((1f - t) * 2f) / 2;
            }
        }

        // Bounce       ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Bounce(float t)
        {
            float p = 0.3f;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
        }

        public static float EaseOut_Bounce(float t)
        {
            return 1 - EaseIn_Bounce(1 - t);
        }

        public static float EaseInOut_Bounce(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Bounce(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Bounce((1f - t) * 2f) / 2;
            }
        }

        // Sinusoidal   ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Sinusoidal(float t)
        {
            return 1 + Mathf.Sin(Mathf.PI / 2f * t - Mathf.PI / 2f);
        }

        public static float EaseOut_Sinusoidal(float t)
        {
            return 1 - EaseIn_Sinusoidal(1 - t);
        }

        public static float EaseInOut_Sinusoidal(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Sinusoidal(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Sinusoidal((1f - t) * 2f) / 2;
            }
        }

        // Overhead     ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Overhead(float t)
        {
            float back = 1.6f;
            return t * t * ((back + 1f) * t - back);
        }

        public static float EaseOut_Overhead(float t)
        {
            return 1 - EaseIn_Overhead(1 - t);
        }

        public static float EaseInOut_Overhead(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Overhead(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Overhead((1f - t) * 2f) / 2;
            }
        }

        // Exponential  ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Exponential(float t)
        {
            return t == 0f ? 0f : Mathf.Pow(1024f, t - 1f);
        }

        public static float EaseOut_Exponential(float t)
        {
            return 1 - EaseIn_Exponential(1 - t);
        }

        public static float EaseInOut_Exponential(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Exponential(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Exponential((1f - t) * 2f) / 2;
            }
        }

        // Elastic      ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Elastic(float t)
        {
            if (t == 0f) { return 0f; }
            if (t == 1f) { return 1f; }
            return -Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t - 0.1f) * (2f * Mathf.PI) / 0.4f);
        }

        public static float EaseOut_Elastic(float t)
        {
            return 1 - EaseIn_Elastic(1 - t);
        }

        public static float EaseInOut_Elastic(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Elastic(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Elastic((1f - t) * 2f) / 2;
            }
        }

        // Circular     ---------------------------------------------------------------------------------------------------------------------------

        public static float EaseIn_Circular(float t)
        {
            return 1f - Mathf.Sqrt(1f - t * t);
        }

        public static float EaseOut_Circular(float t)
        {
            return 1 - EaseIn_Circular(1 - t);
        }

        public static float EaseInOut_Circular(float t)
        {
            if (t < 0.5f)
            {
                return EaseIn_Circular(t * 2f) / 2f;
            }
            else
            {
                return 1 - EaseIn_Circular((1f - t) * 2f) / 2;
            }
        }

    }
}