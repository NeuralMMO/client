using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Fade helpers
	/// </summary>

	public static class MMFade
	{

		/// <summary>
		/// Fades the specified image to the target opacity and duration.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="opacity">Opacity.</param>
		/// <param name="duration">Duration.</param>
		public static IEnumerator FadeImage(Image target, float duration, Color color)
		{
			if (target==null)
				yield break;

			float alpha = target.color.a;

			for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
			{
				if (target==null)
					yield break;
				Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
				target.color=newColor;
				yield return null;
			}
			target.color=color;

		}
		/// <summary>
		/// Fades the specified image to the target opacity and duration.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="opacity">Opacity.</param>
		/// <param name="duration">Duration.</param>
		public static IEnumerator FadeText(Text target, float duration, Color color)
		{
			if (target==null)
				yield break;

			float alpha = target.color.a;

			for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
			{
				if (target==null)
					yield break;
				Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
				target.color=newColor;
				yield return null;
			}			
			target.color=color;
		}
		/// <summary>
		/// Fades the specified image to the target opacity and duration.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="opacity">Opacity.</param>
		/// <param name="duration">Duration.</param>
		public static IEnumerator FadeSprite(SpriteRenderer target, float duration, Color color)
		{
			if (target==null)
				yield break;

			float alpha = target.material.color.a;

			float t=0f;
			while (t<1.0f)
			{
				if (target==null)
					yield break;

				Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
				target.material.color=newColor;

				t += Time.deltaTime / duration;

				yield return null;

			}
			Color finalColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
			if (target != null)
			{
				target.material.color=finalColor;	
			}
		}

		public static IEnumerator FadeCanvasGroup(CanvasGroup target, float duration, float targetAlpha, bool unscaled = true)
		{
			if (target==null)
				yield break;

			float currentAlpha = target.alpha;

			float t=0f;
			while (t<1.0f)
			{
				if (target==null)
					yield break;

				float newAlpha =Mathf.SmoothStep(currentAlpha,targetAlpha,t);
				target.alpha=newAlpha;

				if (unscaled)
				{
					t += Time.unscaledDeltaTime / duration;	
				}
				else
				{
					t += Time.deltaTime / duration;
				}

				yield return null;
			}

			target.alpha=targetAlpha;
		}

	}
}
