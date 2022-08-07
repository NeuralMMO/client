using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MMAudioAnalyzer))]
    [CanEditMultipleObjects]
    public class MMAudioAnalyzerEditor : Editor
    {
        public bool Active;
        public SerializedProperty BandLevels;
        public SerializedProperty BufferedBandLevels;

        public SerializedProperty NormalizedBandLevels;
        public SerializedProperty NormalizedBufferedBandLevels;

        public SerializedProperty BandPeaks;
        public SerializedProperty LastPeaksAt;
        public SerializedProperty RawSpectrum;

        public SerializedProperty Amplitude;
        public SerializedProperty NormalizedAmplitude;
        public SerializedProperty BufferedAmplitude;
        public SerializedProperty NormalizedBufferedAmplitude;

        public SerializedProperty PeaksPasted;

        public SerializedProperty Beats;

        // inspector
        protected float _inspectorWidth;
        protected int _numberOfBands;
        protected Color _barColor;
        protected Color _inactiveColor = new Color(0f, 0f, 0f, 0.4f);
        protected Color _bufferedBarColor = new Color(0f, 0f, 0f, 0.3f);
        protected Color _normalBarColor = MMColors.Orange;
        protected Color _normalNormalizedBarColor = MMColors.Aqua;
        protected Color _peakColor = MMColors.Yellow;
        protected Color _activePeakColor = Color.white;
        protected Color _amplitudeColor = MMColors.DarkOrange;
        protected Color _normalizedAmplitudeColor = MMColors.Aquamarine;
        protected Color _spectrumColor = MMColors.HotPink;
        
        protected Color _beatColor;
        
        protected bool _bandValuesFoldout = false;
        protected float _peakShowDuration = 0.5f;

        // box
        protected Vector2 _boxPosition;
        protected Vector2 _boxSize;
        protected const float _externalMargin = 12f;
        protected float _internalMargin = 12f;
        protected const float _lineHeight = 15f;
        protected const int _numberOfAxis = 5;
        protected const int _numberOfAxisSpectrum = 4;
        protected const int _bandsValuesBoxHeight = 150;
        protected const int _rawSpectrumBoxHeight = 75;


        // coordinates
        protected float _topY;
        protected float _boxBottomY;
        protected float _positionX;
        protected float _positionY;
        
        // column
        protected float _columnWidth;
        protected float _columnHeight;
        protected float _maxColumnHeight;

        // spectrum
        protected float _spectrumBoxBottomY;
        protected Vector2 _spectrumBoxPosition;
        protected Vector2 _spectrumBoxSize;
        protected float _spectrumMaxColumnHeight;

        // axis
        protected Vector3 _axisOrigin = Vector3.zero;
        protected Vector3 _axisDestination = Vector3.zero;

        // styles
        protected GUIStyle _redLabel = new GUIStyle();
        protected Color _normalLabelColor;

        protected Rect _rect;

        protected virtual void OnEnable()
        {
            Active = serializedObject.FindProperty("Active").boolValue;
            BandLevels = serializedObject.FindProperty("BandLevels");
            BufferedBandLevels = serializedObject.FindProperty("BufferedBandLevels");
            NormalizedBandLevels = serializedObject.FindProperty("NormalizedBandLevels");
            NormalizedBufferedBandLevels = serializedObject.FindProperty("NormalizedBufferedBandLevels");
            BandPeaks = serializedObject.FindProperty("BandPeaks");
            LastPeaksAt = serializedObject.FindProperty("LastPeaksAt");
            RawSpectrum = serializedObject.FindProperty("RawSpectrum");
            _numberOfBands = serializedObject.FindProperty("NumberOfBands").intValue;
            Amplitude = serializedObject.FindProperty("Amplitude");
            NormalizedAmplitude = serializedObject.FindProperty("NormalizedAmplitude");
            BufferedAmplitude = serializedObject.FindProperty("BufferedAmplitude");
            NormalizedBufferedAmplitude = serializedObject.FindProperty("NormalizedBufferedAmplitude");
            PeaksPasted = serializedObject.FindProperty("PeaksPasted");
            Beats = serializedObject.FindProperty("Beats");
            _redLabel.normal.textColor = Color.red;
            _rect = new Rect();
        }
        
        /// <summary>
        /// Forces constant repaint of the inspector, making for much faster display of the bands bars.
        /// </summary>
        /// <returns></returns>
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UpdateNumberOfBandsIfNeeded();
            DrawDefaultInspector();
            _inspectorWidth = EditorGUIUtility.currentViewWidth - 24;
            DrawBandTable();
            DrawBeats();
            DrawBandVisualization();
            DrawBandVisualizationNormalized();
            DrawRawSpectrum();
            PreProcessingButtons();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void UpdateNumberOfBandsIfNeeded()
        {
            if (!Application.isPlaying)
            {
                _numberOfBands = serializedObject.FindProperty("NumberOfBands").intValue;
            }
        }

        protected virtual void DrawBandTable()
        {

            GUILayout.Space(10);
            GUILayout.Label("Band values", EditorStyles.boldLabel);

            _bandValuesFoldout = EditorGUILayout.Foldout(_bandValuesFoldout, "Levels");

            if (!Active)
            {
                if (_bandValuesFoldout)
                {
                    GUILayout.Label("Values are only displayed when the game is running.");
                }
                return;
            }

            if (BandPeaks.arraySize == 0)
            {
                return;
            }

            if (_bandValuesFoldout)
            {
                float win = Screen.width;
                float w1 = win * 0.15f;
                float w2 = win * 0.2f;
                float w3 = win * 0.2f;
                float w4 = win * 0.2f;
                float w5 = win * 0.2f;

                float wA = win * 0.5f;
                float wB = win * 0.5f;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Amplitude :", GUILayout.Width(wA));
                GUILayout.Label(Amplitude.floatValue.ToString(), GUILayout.Width(wB));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Normalized Amplitude :", GUILayout.Width(wA));
                GUILayout.Label(NormalizedAmplitude.floatValue.ToString(), GUILayout.Width(wB));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Buffered Amplitude :", GUILayout.Width(wA));
                GUILayout.Label(BufferedAmplitude.floatValue.ToString(), GUILayout.Width(wB));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Normalized Buffered Amplitude :", GUILayout.Width(wA));
                GUILayout.Label(NormalizedBufferedAmplitude.floatValue.ToString(), GUILayout.Width(wB));
                GUILayout.EndHorizontal();



                GUILayout.BeginHorizontal();
                GUILayout.Label("Band", EditorStyles.boldLabel, GUILayout.Width(w1));
                GUILayout.Label("Value", EditorStyles.boldLabel, GUILayout.Width(w2));
                GUILayout.Label("Peak", EditorStyles.boldLabel, GUILayout.Width(w3));
                GUILayout.Label("Normalized", EditorStyles.boldLabel, GUILayout.Width(w4));
                GUILayout.Label("Norm. Buffered", EditorStyles.boldLabel, GUILayout.Width(w5));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                for (int i = 0; i < _numberOfBands; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(i.ToString(), EditorStyles.boldLabel, GUILayout.Width(w1));
                    GUILayout.Label(BandLevels.GetArrayElementAtIndex(i).floatValue.ToString(),  GUILayout.Width(w2));
                    if (Time.time - LastPeaksAt.GetArrayElementAtIndex(i).floatValue < _peakShowDuration)
                    {
                        _normalLabelColor = GUI.skin.label.normal.textColor;
                        GUI.skin.label.normal.textColor = _peakColor;
                        GUILayout.Label(BandPeaks.GetArrayElementAtIndex(i).floatValue.ToString(), GUILayout.Width(w3));
                        GUI.skin.label.normal.textColor = _normalLabelColor;
                    }
                    else
                    {
                        GUILayout.Label(BandPeaks.GetArrayElementAtIndex(i).floatValue.ToString(),  GUILayout.Width(w3));
                    }
                    GUILayout.Label(NormalizedBandLevels.GetArrayElementAtIndex(i).floatValue.ToString(), GUILayout.Width(w4));
                    GUILayout.Label(NormalizedBufferedBandLevels.GetArrayElementAtIndex(i).floatValue.ToString(), GUILayout.Width(w5));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
        }

        protected const int _beatsBoxHeight = 40;

        protected virtual void DrawBeats()
        {
            if ((Beats == null) || (target as MMAudioAnalyzer).Beats == null)
            {
                return;
            }

            float length = (target as MMAudioAnalyzer).Beats.Length;
            if (length <= 0)
            {
                return;
            }

            float margin = _beatsBoxHeight / 10;
            float beatsBoxSquareSize = _beatsBoxHeight - (2 * margin);

            int boxesPerLine = (int)Mathf.Round((_inspectorWidth - margin - 3*_externalMargin) / (beatsBoxSquareSize + margin)) ;
            int numberOfLines = (int)(length / boxesPerLine) + 1;
            float boxHeight = (_beatsBoxHeight) * numberOfLines - margin * (numberOfLines - 1);

            GUILayout.Space(10);
            GUILayout.Label("Beats Visualization", EditorStyles.boldLabel);

            GUILayout.Box("", GUILayout.Width(_inspectorWidth - _externalMargin), GUILayout.Height(boxHeight));

            _boxPosition = GUILayoutUtility.GetLastRect().position;
            _boxSize = GUILayoutUtility.GetLastRect().size;
            _boxBottomY = _boxPosition.y + _beatsBoxHeight - _externalMargin - _lineHeight;

            int counter = 0;
            int lineCounter = 0;
            for (int i = 0; i < length; i++)
            {
                if (counter > boxesPerLine - 1)
                {
                    counter = 0;
                    lineCounter++;
                }

                float boxX = _boxPosition.x + margin + counter * (beatsBoxSquareSize + margin);
                float boxY = _boxPosition.y + margin + lineCounter * (beatsBoxSquareSize + margin);

                // draw bg bar
                _rect.x = boxX;
                _rect.y = boxY;
                _rect.width = beatsBoxSquareSize;
                _rect.height = beatsBoxSquareSize;
                EditorGUI.DrawRect(_rect, _inactiveColor);

                if (Active)
                {
                    // draw front bar 
                    _beatColor = (target as MMAudioAnalyzer).Beats[i].BeatColor;
                    _beatColor.a = (target as MMAudioAnalyzer).Beats[i].CurrentValue;
                    _rect.x = boxX;
                    _rect.y = boxY;
                    _rect.width = beatsBoxSquareSize;
                    _rect.height = beatsBoxSquareSize;
                    EditorGUI.DrawRect(_rect, _beatColor);
                }

                // draw number
                float labelX = (i > 9) ? boxX + beatsBoxSquareSize / 4 - 2 : boxX + beatsBoxSquareSize / 4 + 2;
                
                _rect.x = labelX;
                _rect.y = boxY + beatsBoxSquareSize / 4;
                _rect.width = beatsBoxSquareSize;
                _rect.height = beatsBoxSquareSize;
                EditorGUI.LabelField(_rect, i.ToString(), EditorStyles.boldLabel);

                counter++;
            }
        }

        protected virtual void DrawBandVisualization()
        {
            GUILayout.Space(10);
            GUILayout.Label("Raw Visualization", EditorStyles.boldLabel);

            _internalMargin = (_numberOfBands > 8) ? 6f : _externalMargin;

            // box
            GUILayout.Box("", GUILayout.Width(_inspectorWidth - _externalMargin), GUILayout.Height(_bandsValuesBoxHeight));
            _boxPosition = GUILayoutUtility.GetLastRect().position;
            _boxSize = GUILayoutUtility.GetLastRect().size;
            _boxBottomY = _boxPosition.y + _boxSize.y - _externalMargin - _lineHeight;
            _columnWidth = (_boxSize.x - (_numberOfBands + 1) * _internalMargin) / _numberOfBands;
            _maxColumnHeight = _boxSize.y - 2 * _externalMargin - _lineHeight - 5;

            // lines
            Handles.BeginGUI();

            // horizontal axis
            Handles.color = Color.grey;
            for (int i = 0; i < _numberOfAxis; i++)
            {
                _axisOrigin.x = _boxPosition.x;
                _axisOrigin.y = _boxBottomY + _lineHeight / _numberOfAxis - i * (_boxSize.y / _numberOfAxis);
                _axisDestination.x = _boxPosition.x + _boxSize.x;
                _axisDestination.y = _axisOrigin.y;
                Handles.DrawLine(_axisOrigin, _axisDestination);
            }

            // peaks
            if ((BandPeaks != null) && (BandPeaks.arraySize == _numberOfBands))
            {
                for (int i = 0; i < _numberOfBands; i++)
                {
                    float peak = BandPeaks.GetArrayElementAtIndex(i).floatValue;
                    if (Active)
                    {
                        if (Time.time - LastPeaksAt.GetArrayElementAtIndex(i).floatValue < _peakShowDuration)
                        {
                            Handles.color = _activePeakColor;
                        }
                        else
                        {
                            Handles.color = _peakColor;
                        }
                    }
                    else
                    {
                        Handles.color = _peakColor;
                    }                    
                    _axisOrigin.x = _boxPosition.x + _internalMargin * (i + 1) + _columnWidth * i;
                    _axisOrigin.y = _boxBottomY - MMMaths.Remap(peak, 0f, 1f, 0f, _maxColumnHeight);
                    _axisDestination.x = _axisOrigin.x + _columnWidth;
                    _axisDestination.y = _axisOrigin.y;
                    Handles.DrawLine(_axisOrigin, _axisDestination);
                }
            }

            Handles.EndGUI();

            // amplitude cursors
            _columnHeight = MMMaths.Remap(Amplitude.floatValue, 0f, 1f, 0f, _maxColumnHeight);
            _positionX = _boxPosition.x - _externalMargin/4 ;
            _positionY = _boxBottomY - _columnHeight;

            _rect.x = _positionX;
            _rect.y = _positionY;
            _rect.width = _externalMargin / 2;
            _rect.height = _externalMargin / 2;
            EditorGUI.DrawRect(_rect, _amplitudeColor);

            _columnHeight = MMMaths.Remap(BufferedAmplitude.floatValue, 0f, 1f, 0f, _maxColumnHeight);
            _positionX = _boxPosition.x + _boxSize.x - _externalMargin / 4;
            _positionY = _boxBottomY - _columnHeight;

            _rect.x = _positionX;
            _rect.y = _positionY;
            _rect.width = _externalMargin / 2;
            _rect.height = _externalMargin / 2;
            EditorGUI.DrawRect(_rect, _amplitudeColor);

            // buffered bars
            for (int i = 0; i < _numberOfBands; i++)
            {
                if (Active)
                {
                    float bandLevel = BufferedBandLevels.GetArrayElementAtIndex(i).floatValue;
                    _columnHeight = MMMaths.Remap(bandLevel, 0f, 1f, 0f, _maxColumnHeight);
                    _barColor = (Time.time - LastPeaksAt.GetArrayElementAtIndex(i).floatValue < _peakShowDuration/3f) ? _activePeakColor : _bufferedBarColor;

                    _positionX = _boxPosition.x + _internalMargin * (i + 1) + _columnWidth * i;
                    _positionY = _boxBottomY;

                    // bar rectangle
                    _rect.x = _positionX;
                    _rect.y = _positionY;
                    _rect.width = _columnWidth;
                    _rect.height = -_columnHeight;
                    EditorGUI.DrawRect(_rect, _barColor);
                }
            }

            // bars
            for (int i = 0; i < _numberOfBands; i++)
            {
                if (Active)
                {
                    float bandLevel = BandLevels.GetArrayElementAtIndex(i).floatValue;
                    _columnHeight = MMMaths.Remap(bandLevel, 0f, 1f, 0f, _maxColumnHeight);
                    _barColor = (Time.time - LastPeaksAt.GetArrayElementAtIndex(i).floatValue < _peakShowDuration) ? _peakColor : _normalBarColor;
                }
                else
                {
                    _barColor = _inactiveColor;
                    _columnHeight = (i + 1) * (_maxColumnHeight / (_numberOfBands + 1));
                }

                _positionX = _boxPosition.x + _internalMargin * (i + 1) + _columnWidth * i;
                _positionY = _boxBottomY;

                // bar rectangle
                _rect.x = _positionX;
                _rect.y = _positionY;
                _rect.width = _columnWidth;
                _rect.height = -_columnHeight;
                EditorGUI.DrawRect(_rect, _barColor);
                // bar number label
                float labelCorrection = (i > 9) ? -5f : 0f;
                _rect.x = _positionX + _columnWidth / 2 - 5 + labelCorrection;
                _rect.y = _boxBottomY + _lineHeight / 4;
                _rect.width = _columnWidth;
                _rect.height = _lineHeight;
                EditorGUI.LabelField(_rect, i.ToString(), EditorStyles.boldLabel);
            }
        }

        protected virtual void DrawBandVisualizationNormalized()
        {
            GUILayout.Space(10);
            GUILayout.Label("Normalized Visualization", EditorStyles.boldLabel);

            // box
            GUILayout.Box("", GUILayout.Width(_inspectorWidth - _externalMargin), GUILayout.Height(_bandsValuesBoxHeight));
            _boxPosition = GUILayoutUtility.GetLastRect().position;
            _boxSize = GUILayoutUtility.GetLastRect().size;
            _boxBottomY = _boxPosition.y + _boxSize.y - _externalMargin - _lineHeight;
            _columnWidth = (_boxSize.x - (_numberOfBands + 1) * _internalMargin) / _numberOfBands;
            _maxColumnHeight = _boxSize.y - 2 * _externalMargin - _lineHeight;

            // lines
            Handles.BeginGUI();

            // horizontal axis
            Handles.color = Color.grey;
            for (int i = 0; i < _numberOfAxis; i++)
            {
                _axisOrigin.x = _boxPosition.x;
                _axisOrigin.y = _boxBottomY + _lineHeight / _numberOfAxis - i * (_boxSize.y / _numberOfAxis);
                _axisDestination.x = _boxPosition.x + _boxSize.x;
                _axisDestination.y = _axisOrigin.y;
                Handles.DrawLine(_axisOrigin, _axisDestination);
            }

            Handles.EndGUI();


            // amplitude cursors
            _columnHeight = MMMaths.Remap(NormalizedAmplitude.floatValue, 0f, 1f, 0f, _maxColumnHeight);
            _positionX = _boxPosition.x - _externalMargin / 4;
            _positionY = _boxBottomY - _columnHeight;

            _rect.x = _positionX;
            _rect.y = _positionY;
            _rect.width = _externalMargin / 2;
            _rect.height = _externalMargin / 2;
            EditorGUI.DrawRect(_rect, _normalizedAmplitudeColor);

            _columnHeight = MMMaths.Remap(NormalizedBufferedAmplitude.floatValue, 0f, 1f, 0f, _maxColumnHeight);
            _positionX = _boxPosition.x + _boxSize.x - _externalMargin / 4;
            _positionY = _boxBottomY - _columnHeight;

            _rect.x = _positionX;
            _rect.y = _positionY;
            _rect.width = _externalMargin / 2;
            _rect.height = _externalMargin / 2;
            EditorGUI.DrawRect(_rect, _normalizedAmplitudeColor);

            // buffered bars
            for (int i = 0; i < _numberOfBands; i++)
            {
                if (Active)
                {
                    float bandLevel = NormalizedBufferedBandLevels.GetArrayElementAtIndex(i).floatValue;
                    _columnHeight = MMMaths.Remap(bandLevel, 0f, 1f, 0f, _maxColumnHeight);
                    _barColor = (Time.time - LastPeaksAt.GetArrayElementAtIndex(i).floatValue < _peakShowDuration / 3f) ? _activePeakColor : _bufferedBarColor;

                    _positionX = _boxPosition.x + _internalMargin * (i + 1) + _columnWidth * i;
                    _positionY = _boxBottomY;

                    // bar rectangle
                    _rect.x = _positionX;
                    _rect.y = _positionY;
                    _rect.width = _columnWidth;
                    _rect.height = -_columnHeight;
                    EditorGUI.DrawRect(_rect, _barColor);
                }
            }

            // bars
            for (int i = 0; i < _numberOfBands; i++)
            {
                if (Active)
                {
                    float bandLevel = NormalizedBandLevels.GetArrayElementAtIndex(i).floatValue;
                    _columnHeight = MMMaths.Remap(bandLevel, 0f, 1f, 0f, _maxColumnHeight);
                    _barColor = (Time.time - LastPeaksAt.GetArrayElementAtIndex(i).floatValue < _peakShowDuration) ? _peakColor : _normalNormalizedBarColor;
                }
                else
                {
                    _barColor = _inactiveColor;
                    _columnHeight = (i + 1) * (_maxColumnHeight / (_numberOfBands + 1));
                }

                _positionX = _boxPosition.x + _internalMargin * (i + 1) + _columnWidth * i;
                _positionY = _boxBottomY;

                // bar rectangle
                _rect.x = _positionX;
                _rect.y = _positionY;
                _rect.width = _columnWidth;
                _rect.height = -_columnHeight;
                EditorGUI.DrawRect(_rect, _barColor);
                // bar number label
                float labelCorrection = (i > 9) ? -5f : 0f;

                _rect.x = _positionX + _columnWidth / 2 - 5 + labelCorrection;
                _rect.y = _boxBottomY + _lineHeight / 4;
                _rect.width = _columnWidth;
                _rect.height = _lineHeight;
                EditorGUI.LabelField(_rect, i.ToString(), EditorStyles.boldLabel);
            }
        }

        protected virtual void DrawRawSpectrum()
        {
            GUILayout.Space(10);
            GUILayout.Label("Raw Spectrum", EditorStyles.boldLabel);

            // box
            GUILayout.Box("", GUILayout.Width(_inspectorWidth - _externalMargin), GUILayout.Height(_rawSpectrumBoxHeight));
            _spectrumBoxPosition = GUILayoutUtility.GetLastRect().position;
            _spectrumBoxSize = GUILayoutUtility.GetLastRect().size;
            _spectrumBoxBottomY = _spectrumBoxPosition.y + _spectrumBoxSize.y;
            _spectrumMaxColumnHeight = _spectrumBoxSize.y - 2 * _externalMargin;
            Handles.BeginGUI();

            // horizontal axis
            Handles.color = Color.grey;
            for (int i = 0; i < _numberOfAxisSpectrum; i++)
            {
                _axisOrigin.x = _spectrumBoxPosition.x;
                _axisOrigin.y = _spectrumBoxBottomY - i * (_spectrumBoxSize.y / _numberOfAxisSpectrum);
                _axisDestination.x = _spectrumBoxPosition.x + _spectrumBoxSize.x;
                _axisDestination.y = _axisOrigin.y;
                Handles.DrawLine(_axisOrigin, _axisDestination);
            }

            if (Active)
            {
                // spectrum
                for (int i = 1; i < RawSpectrum.arraySize - 1; i++)
                {

                    float xPosition = _spectrumBoxPosition.x + _externalMargin + MMMaths.Remap(i, 0, RawSpectrum.arraySize, 0f, _spectrumBoxSize.x - _externalMargin * 2);
                    float yPosition = _spectrumBoxPosition.y + _spectrumBoxSize.y / 2 ;
                    float deltaX = (_spectrumBoxSize.x - _externalMargin * 2) / RawSpectrum.arraySize;

                    float spectrumValue = RawSpectrum.GetArrayElementAtIndex(i).floatValue;
                    float spectrumValuePrevious = RawSpectrum.GetArrayElementAtIndex(i - 1).floatValue;

                    float factor = _spectrumBoxSize.y/2;

                    spectrumValue = - (1 / Mathf.Log(spectrumValue)) * factor; ;
                    spectrumValuePrevious = - (1 / Mathf.Log(spectrumValuePrevious)) * factor;

                    spectrumValue = Mathf.Clamp(spectrumValue, 0f, _spectrumBoxSize.y / 2f);
                    spectrumValuePrevious = Mathf.Clamp(spectrumValuePrevious, 0f, _spectrumBoxSize.y / 2f);

                    Handles.color = _spectrumColor;
                    _axisOrigin.x = xPosition - deltaX;
                    _axisOrigin.y = yPosition + spectrumValuePrevious;
                    _axisDestination.x = xPosition;
                    _axisDestination.y = (i % 2 == 0) ? yPosition + spectrumValue : yPosition - spectrumValue;
                    Handles.DrawLine(_axisOrigin, _axisDestination);
                }
            }
            else
            {
                int points = 100;
                for (int i = 1; i < points - 1; i++)
                {
                    float xPosition = _spectrumBoxPosition.x + _externalMargin + MMMaths.Remap(i, 0, points, 0f, _spectrumBoxSize.x - _externalMargin * 2);
                    float yPosition = _spectrumBoxPosition.y + _spectrumBoxSize.y / 2;
                    float deltaX = (_spectrumBoxSize.x - _externalMargin * 2) / points;

                    float spectrumValue = Foobar(i);
                    float spectrumValuePrevious = Foobar(i-1);

                    float factor = _spectrumBoxSize.y / 2;

                    Handles.color = _inactiveColor;
                    _axisOrigin.x = xPosition - deltaX;
                    _axisOrigin.y = yPosition + spectrumValuePrevious;
                    _axisDestination.x = xPosition;
                    _axisDestination.y = yPosition + spectrumValue ;
                    Handles.DrawLine(_axisOrigin, _axisDestination);
                }
            }
            Handles.EndGUI();
        }

        protected virtual float Foobar(float x)
        {
            return 25f * Mathf.Sin(x * 0.5f);
        }

        protected virtual void PreProcessingButtons()
        {
            if ((target as MMAudioAnalyzer).Mode != MMAudioAnalyzer.Modes.AudioSource)
            {
                return;
            }

            GUILayout.Space(10);
            GUILayout.Label("Peaks preprocessing", EditorStyles.boldLabel);
            
            if (!PeaksPasted.boolValue)
            {
                if ((PeaksSaver.Peaks == null) || (PeaksSaver.Peaks.Length == 0))
                {
                    EditorGUILayout.HelpBox("You haven't preprocessed peaks for this track yet. It's recommended to do so, by pressing play, " +
                        "then the Find Peaks button below. Then, exit play, and press the 'Paste Peaks' button.", MessageType.Warning);
                    if (GUILayout.Button("Find Peaks"))
                    {
                        (target as MMAudioAnalyzer).FindPeaks();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Exit Play Mode first, then paste your saved peaks using the 'Paste Peaks' button below.", MessageType.Warning);
                    if (!Application.isPlaying)
                    {
                        if (GUILayout.Button("Paste Peaks"))
                        {
                            (target as MMAudioAnalyzer).PastePeaks();
                        }
                    }                    
                }
            }            
            if (GUILayout.Button("Clear Peaks"))
            {
                (target as MMAudioAnalyzer).ClearPeaks();
            }        
        }
    }
#endif
}
