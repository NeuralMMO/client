using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Rendering;

namespace MoreMountains.TopDownEngine
{
    [CustomEditor(typeof(MMPlaylist))]
    [CanEditMultipleObjects]

    /// <summary>
    /// A custom editor that displays the current state of a playlist when the game is running
    /// </summary>
    public class MMPlaylistEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MMPlaylist playlist = (MMPlaylist)target;

            DrawDefaultInspector();

            if (playlist.PlaylistState != null)
            {
                EditorGUILayout.LabelField("Current State", playlist.PlaylistState.CurrentState.ToString());
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}