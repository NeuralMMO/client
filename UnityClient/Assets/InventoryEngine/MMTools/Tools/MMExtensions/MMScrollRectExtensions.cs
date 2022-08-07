using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Scrollrect extensions
    /// </summary>
    public static class ScrollRectExtensions
    {
        /// <summary>
        /// Scrolls a scroll rect to the top
        /// </summary>
        /// <param name="scrollRect"></param>
        public static void MMScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        /// <summary>
        /// Scrolls a scroll rect to the bottom
        /// </summary>
        public static void MMScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
}
