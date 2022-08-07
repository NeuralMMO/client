using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	public static class MMGUI 
	{
		public static void SetSize(RectTransform rectTransform, Vector2 newSize) 
		{
	        Vector2 currSize = rectTransform.rect.size;
	        Vector2 sizeDiff = newSize - currSize;
	        rectTransform.offsetMin = rectTransform.offsetMin - new Vector2(sizeDiff.x * rectTransform.pivot.x, sizeDiff.y * rectTransform.pivot.y);
	        rectTransform.offsetMax = rectTransform.offsetMax + new Vector2(sizeDiff.x * (1.0f - rectTransform.pivot.x), sizeDiff.y * (1.0f - rectTransform.pivot.y));
	    }
	}
}