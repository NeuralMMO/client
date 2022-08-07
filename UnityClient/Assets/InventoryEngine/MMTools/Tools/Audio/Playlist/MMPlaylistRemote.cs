using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class used to pilot a MMPlaylist
    /// </summary>
    public class MMPlaylistRemote : MonoBehaviour
    {
        /// The track to play when calling PlaySelectedTrack
        public int TrackNumber = 0;

        [Header("Triggers")]
        /// if this is true, the selected track will be played on trigger enter (if you have a trigger collider on this)
        public bool PlaySelectedTrackOnTriggerEnter = true;
        /// if this is true, the selected track will be played on trigger exit (if you have a trigger collider on this)
        public bool PlaySelectedTrackOnTriggerExit = false;
        /// the tag to check for on trigger stuff
        public string TriggerTag = "Player";

        [Header("Test")]
        /// a play test button
        [MMInspectorButton("Play")]
        public bool PlayButton;
        /// a pause test button
        [MMInspectorButton("Pause")]
        public bool PauseButton;
        /// a stop test button
        [MMInspectorButton("Stop")]
        public bool StopButton;
        /// a next track test button
        [MMInspectorButton("PlayNextTrack")]
        public bool NextButton;
        /// a selected track test button
        [MMInspectorButton("PlaySelectedTrack")]
        public bool SelectedTrackButton;

        /// <summary>
        /// Plays the playlist
        /// </summary>
        public virtual void Play()
        {
            MMPlaylistPlayEvent.Trigger();
        }

        /// <summary>
        /// Pauses the current track
        /// </summary>
        public virtual void Pause()
        {
            MMPlaylistPauseEvent.Trigger();
        }

        /// <summary>
        /// Stops the playlist
        /// </summary>
        public virtual void Stop()
        {
            MMPlaylistStopEvent.Trigger();
        }

        /// <summary>
        /// Plays the next track in the playlist
        /// </summary>
        public virtual void PlayNextTrack()
        {
            MMPlaylistPlayNextEvent.Trigger();
        }

        /// <summary>
        /// Plays the track selected in the inspector
        /// </summary>
        public virtual void PlaySelectedTrack()
        {
            MMPlaylistPlayIndexEvent.Trigger(TrackNumber);
        }

        /// <summary>
        /// Plays the track set in parameters
        /// </summary>
        public virtual void PlayTrack(int trackIndex)
        {
            MMPlaylistPlayIndexEvent.Trigger(trackIndex);
        }

        /// <summary>
        /// On trigger enter, we play the selected track if needed
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (PlaySelectedTrackOnTriggerEnter && (collider.CompareTag(TriggerTag)))
            {
                PlaySelectedTrack();
            }
        }

        /// <summary>
        /// On trigger exit, we play the selected track if needed
        /// </summary>
        protected virtual void OnTriggerExit(Collider collider)
        {
            if (PlaySelectedTrackOnTriggerExit && (collider.CompareTag(TriggerTag)))
            {
                PlaySelectedTrack();
            }
        }
    }
}
