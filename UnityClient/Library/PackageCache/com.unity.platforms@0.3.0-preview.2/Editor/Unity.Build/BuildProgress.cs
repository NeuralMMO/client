using System;
using UnityEditor;

namespace Unity.Build
{
    /// <summary>
    /// Scoped progress indicator that will clear itself on dispose.
    /// </summary>
    public sealed class BuildProgress : IDisposable
    {
        static int s_Depth;
        static TimeSpan s_UpdateFrequency = TimeSpan.FromMilliseconds(250);

        string m_Title;
        string m_Info;
        float m_Percent;
        bool m_Cancelled;
        DateTime m_LastUpdateTime;

        /// <summary>
        /// Update or get the title of the progress indicator.
        /// </summary>
        public string Title
        {
            get => m_Title;
            set
            {
                if (value == m_Title)
                {
                    return;
                }

                m_Title = value;
                Update();
            }
        }

        /// <summary>
        /// Update or get the information of the progress indicator.
        /// </summary>
        public string Info
        {
            get => m_Info;
            set
            {
                if (value == m_Info)
                {
                    return;
                }

                m_Info = value;
                Update();
            }
        }

        /// <summary>
        /// Update or get the completion percent of the progress indicator.
        /// </summary>
        public float Percent
        {
            get => m_Percent;
            set
            {
                var percent = Clamp(value, 0f, 1f);
                if (percent == m_Percent)
                {
                    return;
                }

                m_Percent = percent;
                Update();
            }
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="BuildProgress"/> class, which acts as a scoped progress indicator.
        /// </summary>
        /// <param name="title">Title of the progress indicator.</param>
        /// <param name="info">Information of the progress indicator.</param>
        /// <param name="percent">Completion percent of the progress indicator.</param>
        public BuildProgress(string title, string info, float percent = 0f)
        {
            s_Depth++;
            Update(title, info, percent);
        }

        /// <summary>
        /// Update the progress indicator. Returns <see langword="true"/> if user cancelled.
        /// </summary>
        /// <param name="title">Title of the progress indicator.</param>
        /// <param name="info">Information of the progress indicator.</param>
        /// <param name="percent">Completion percent of the progress indicator.</param>
        /// <returns><see langword="true"/> if the user cancelled, <see langword="false"/> otherwise.</returns>
        public bool Update(string title, string info, float percent)
        {
            title = title ?? m_Title ?? string.Empty;
            info = info ?? m_Info ?? string.Empty;
            percent = Clamp(percent, 0f, 1f);

            var now = DateTime.Now;
            var elapsed = now - m_LastUpdateTime;
            if (elapsed >= s_UpdateFrequency || title != m_Title || info != m_Info || percent != m_Percent)
            {
                m_Title = title;
                m_Info = info;
                m_Percent = percent;
                m_LastUpdateTime = now;
                m_Cancelled = EditorUtility.DisplayCancelableProgressBar(m_Title, m_Info, m_Percent);
            }

            return m_Cancelled;
        }

        /// <summary>
        /// Update the progress indicator. Returns <see langword="true"/> if user cancelled.
        /// </summary>
        /// <param name="info">Information of the progress indicator.</param>
        /// <param name="percent">Completion percent of the progress indicator.</param>
        /// <returns><see langword="true"/> if the user cancelled, <see langword="false"/> otherwise.</returns>
        public bool Update(string info, float percent) => Update(m_Title, info, percent);

        /// <summary>
        /// Update the progress indicator. Returns <see langword="true"/> if user cancelled.
        /// </summary>
        /// <param name="info">Information of the progress indicator.</param>
        /// <param name="percent">Completion percent of the progress indicator.</param>
        /// <returns><see langword="true"/> if the user cancelled, <see langword="false"/> otherwise.</returns>
        public bool Update(float percent) => Update(m_Title, m_Info, percent);

        /// <summary>
        /// Update the progress indicator. Returns <see langword="true"/> if user cancelled.
        /// </summary>
        /// <returns><see langword="true"/> if the user cancelled, <see langword="false"/> otherwise.</returns>
        public bool Update() => Update(m_Title, m_Info, m_Percent);

        public void Dispose()
        {
            if (--s_Depth == 0)
            {
                EditorUtility.ClearProgressBar();
            }
        }

        float Clamp(float value, float min, float max) => Math.Max(Math.Min(value, max), min);
    }
}
