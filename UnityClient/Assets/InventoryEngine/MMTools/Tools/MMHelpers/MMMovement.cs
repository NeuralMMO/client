using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Movement helpers
	/// </summary>

	public static class MMMovement 
	{

		/// <summary>
		/// Moves an object from point A to point B in a given time
		/// </summary>
		/// <param name="movingObject">Moving object.</param>
		/// <param name="pointA">Point a.</param>
		/// <param name="pointB">Point b.</param>
		/// <param name="duration">Time.</param>
		public static IEnumerator MoveFromTo(GameObject movingObject,Vector3 pointA, Vector3 pointB, float duration, AnimationCurve curve = null)
		{	                    
            float journey = 0f;
            Vector3 newPosition;

            while (journey < duration)
            {
                float percent = Mathf.Clamp01(journey / duration);

                newPosition = Vector3.Lerp(pointA, pointB, curve.Evaluate(percent));

                movingObject.transform.position = newPosition;

                journey += Time.deltaTime;
                yield return null;
			}
	        yield break;
        }

        public static IEnumerator AnimateScale(Transform targetTransform, Vector3 vector, float duration, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float multiplier = 1f)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            if ((curveX == null) || (curveY == null) || (curveZ == null))
            {
                yield break;
            }

            if (duration == 0f)
            {
                yield break;
            }

            float journey = 0f;

            while (journey < duration)
            {
                float percent = Mathf.Clamp01(journey / duration);

                vector.x = curveX.Evaluate(percent);
                vector.y = curveY.Evaluate(percent);
                vector.z = curveZ.Evaluate(percent);
                targetTransform.localScale = multiplier * vector;

                journey += Time.deltaTime;
                yield return null;
            }
            yield return null;
        }

        public static IEnumerator AnimateRotation(Transform targetTransform, 
                                                    Vector3 vector, 
                                                    float duration, 
                                                    AnimationCurve curveX, 
                                                    AnimationCurve curveY, 
                                                    AnimationCurve curveZ,
                                                    float multiplier)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            if ((curveX == null) || (curveY == null) || (curveZ == null))
            {
                yield break;
            }

            if (duration == 0f)
            {
                yield break;
            }

            float journey = 0f;

            while (journey < duration)
            {
                float percent = Mathf.Clamp01(journey / duration);

                vector.x = curveX.Evaluate(percent) * multiplier;
                vector.y = curveY.Evaluate(percent) * multiplier;
                vector.z = curveZ.Evaluate(percent) * multiplier;
                targetTransform.localEulerAngles = vector;

                journey += Time.deltaTime;
                yield return null;
            }
            yield return null;
        }

    }
}