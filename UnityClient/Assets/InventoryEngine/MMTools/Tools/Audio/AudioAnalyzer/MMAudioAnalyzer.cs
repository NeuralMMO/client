using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using System;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A static class used to save / load peaks once they've been computed
    /// </summary>
    public static class PeaksSaver
    {
        public static float[] Peaks;
    }

    /// <summary>
    /// An event you can listen to that will get automatically triggered for every remapped beat
    /// </summary>
    public struct MMBeatEvent
    {
        public delegate void Delegate(string name, float value);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(string name, float value)
        {
            OnEvent?.Invoke(name, value);
        }
    }

    [Serializable]
    public class Beat
    {
        public string Name = "Beat";
        public enum Modes { Raw, Normalized, BufferedRaw, BufferedNormalized, Amplitude, NormalizedAmplitude, AmplitudeBuffered, NormalizedAmplitudeBuffered }
        // remapped will send beat events when a threshold is passed, live just updates the value with whatever value is reading right now
        public enum BeatValueModes { Remapped, Live }

        public Modes Mode = Modes.BufferedNormalized;
        public BeatValueModes BeatValueMode = BeatValueModes.Remapped;

        [MMEnumCondition("Mode", (int)Modes.Raw, (int)Modes.Normalized, (int)Modes.BufferedRaw, (int)Modes.BufferedNormalized)]
        public Color BeatColor = Color.cyan;
        public int BandID = 0;
        public float Threshold = 0.5f;
        public float MinimumTimeBetweenBeats = 0.25f;

        [MMEnumCondition("BeatValueMode", (int)BeatValueModes.Remapped)]
        public float RemappedAttack = 0.05f;
        [MMEnumCondition("BeatValueMode", (int)BeatValueModes.Remapped)]
        public float RemappedDecay = 0.2f;

        [MMReadOnly]
        public bool BeatThisFrame;
        [MMReadOnly]
        public float CurrentValue;
        [HideInInspector]
        public float _previousValue;
        [HideInInspector]
        public float _lastBeatAt;
        [HideInInspector]
        public float _lastBeatValue;
        [HideInInspector]
        public bool _initialized = false;

        public UnityEvent OnBeat;

        public void InitializeIfNeeded(int id, int bandID)
        {
            if (!_initialized)
            {
                Mode = Modes.Normalized;
                BeatValueMode = BeatValueModes.Remapped;
                Name = "Beat " + id;
                BeatColor = MMColors.RandomColor();
                BandID = bandID;
                Threshold = 0.3f + id * 0.02f;
                if (Threshold > 0.6f) { Threshold -= 0.5f; }
                Threshold = Threshold % 1f;
                MinimumTimeBetweenBeats = 0.25f + id * 0.02f;
                RemappedAttack = 0.05f + id * 0.01f;
                RemappedDecay = 0.2f + id * 0.01f;
                _initialized = true;
            }
        }
    }

    public class MMAudioAnalyzer : MonoBehaviour
    {
        public enum Modes { Global, AudioSource, Microphone }

        [Header("Source")]
        [MMInformation("This component lets you pick an audio source (either global : the whole scene's audio, a unique source, or the " +
            "microphone), and will cut it into chunks that you can then use to emit beat events, that other objects can consume and act upon. " +
            "The sample interval is the frequency at which sound will be analyzed, the amount of spectrum samples will determine the " +
            "accuracy of the sampling, the window defines the method used to reduce leakage, and the number of bands " +
            "will determine in how many bands you want to cut the sound. The more bands, the more levers you'll have to play with afterwards." +
            "In general, for all of these settings, higher values mean better quality and lower performance. The buffer speed determines how " +
            "fast buffered band levels readjust.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        [MMReadOnlyWhenPlaying]
        public Modes Mode = Modes.Global;
        [MMEnumCondition("Mode", (int)Modes.AudioSource)]
        [MMReadOnlyWhenPlaying]
        public AudioSource TargetAudioSource;
        [MMEnumCondition("Mode", (int)Modes.Microphone)]
        public int MicrophoneID = 0;

        [Header("Sampling")]
        [MMReadOnlyWhenPlaying]
        public float SampleInterval = 0.02f;
        [MMDropdown(2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192)]
        [MMReadOnlyWhenPlaying]
        public int SpectrumSamples = 1024;
        [MMReadOnlyWhenPlaying]
        public FFTWindow Window = FFTWindow.Rectangular;
        [Range(1, 64)]
        [MMReadOnlyWhenPlaying]
        public int NumberOfBands = 8;
        public float BufferSpeed = 2f;

        [Header("Beat Events")]
        public Beat[] Beats;

        [HideInInspector]
        public float[] RawSpectrum;

        [HideInInspector]
        public float[] BandLevels;
        [HideInInspector]
        public float[] BufferedBandLevels;

        [HideInInspector]
        public float[] BandPeaks;
        [HideInInspector]
        public float[] LastPeaksAt;

        [HideInInspector]
        public float[] NormalizedBandLevels;
        [HideInInspector]
        public float[] NormalizedBufferedBandLevels;

        [HideInInspector]
        public float Amplitude;
        [HideInInspector]
        public float NormalizedAmplitude;
        [HideInInspector]
        public float BufferedAmplitude;
        [HideInInspector]
        public float NormalizedBufferedAmplitude;
        [HideInInspector]
        public bool Active = false;
        [HideInInspector]
        public bool PeaksPasted = false;

        protected const int _microphoneDuration = 5;
        protected string _microphone;
        protected float _microphoneStartedAt = 0f;
        protected const float _microphoneDelay = 0.030f;
        protected const float _microphoneFrequency = 24000f;
        protected WaitForSeconds _sampleIntervalWaitForSeconds;
        protected int _cachedNumberOfBands;

        public virtual void FindPeaks()
        {
            float time = 0f;
            while (time < TargetAudioSource.clip.length)
            {
                TargetAudioSource.time = time;
                TargetAudioSource.GetSpectrumData(RawSpectrum, 0, Window);
                time += SampleInterval;
                ComputeBandLevels();
                PeaksSaver.Peaks = BandPeaks;
            }
        }

        public virtual void PastePeaks()
        {
            BandPeaks = PeaksSaver.Peaks;
            PeaksSaver.Peaks = null;
            PeaksPasted = true;
        }

        public virtual void ClearPeaks()
        {
            BandPeaks = null;
            PeaksSaver.Peaks = null;
            PeaksPasted = false;
        }

        protected virtual void Awake()
        {
            Initialization();
        }

        public virtual void Initialization()
        {
            _cachedNumberOfBands = NumberOfBands;
            RawSpectrum = new float[SpectrumSamples];
            BandLevels = new float[_cachedNumberOfBands];
            BufferedBandLevels = new float[_cachedNumberOfBands];

            // we make sure our peaks match our bands
            if ((BandPeaks == null) || (BandPeaks.Length == 0))
            {
                BandPeaks = new float[_cachedNumberOfBands];
                PeaksPasted = false;
            }
            if (BandPeaks.Length != BandLevels.Length)
            {
                BandPeaks = new float[_cachedNumberOfBands];
                PeaksPasted = false;
            }
            LastPeaksAt = new float[_cachedNumberOfBands];
            NormalizedBandLevels = new float[_cachedNumberOfBands];
            NormalizedBufferedBandLevels = new float[_cachedNumberOfBands];

            if ((Mode == Modes.AudioSource) && (TargetAudioSource == null))
            {
                Debug.LogError(this.name + " : this MMAudioAnalyzer needs a target audio source to operate.");
                return;
            }

            if (Mode == Modes.Microphone)
            {
#if !UNITY_WEBGL
                GameObject audioSourceGo = new GameObject("Microphone");
                audioSourceGo.transform.SetParent(this.gameObject.transform);
                TargetAudioSource = audioSourceGo.AddComponent<AudioSource>();                
                string _microphone = Microphone.devices[MicrophoneID].ToString();
                TargetAudioSource.clip = Microphone.Start(_microphone, true, _microphoneDuration, (int)_microphoneFrequency);
                TargetAudioSource.Play();
                _microphoneStartedAt = Time.time;
#endif
            }

            Active = true;
            _sampleIntervalWaitForSeconds = new WaitForSeconds(SampleInterval);
            StartCoroutine(Analyze());
        }

        protected virtual void Update()
        {
            HandleBuffer();
            ComputeAmplitudes();
            HandleBeats();
        }

        protected virtual IEnumerator Analyze()
        {
            while (true)
            {
                switch (Mode)
                {
                    case Modes.AudioSource:
                        TargetAudioSource.GetSpectrumData(RawSpectrum, 0, Window);
                        break;
                    case Modes.Global:
                        AudioListener.GetSpectrumData(RawSpectrum, 0, Window);
                        break;
                    case Modes.Microphone:
#if !UNITY_WEBGL
                        int microphoneSamples = Microphone.GetPosition(_microphone);
                        if (microphoneSamples / _microphoneFrequency > _microphoneDelay)
                        {
                            if (!TargetAudioSource.isPlaying)
                            {
                                TargetAudioSource.timeSamples = (int)(microphoneSamples - (_microphoneDelay * _microphoneFrequency));
                                TargetAudioSource.Play();
                            }
                            _microphoneStartedAt = Time.time;
                        }
                        AudioListener.GetSpectrumData(RawSpectrum, 0, Window);
#endif
                        break;
                }

                ComputeBandLevels();
                yield return _sampleIntervalWaitForSeconds;
            }
        }

        protected virtual void HandleBuffer()
        {
            for (int i = 0; i < BandLevels.Length; i++)
            {
                BufferedBandLevels[i] = Mathf.Max(BufferedBandLevels[i] * Mathf.Exp(-BufferSpeed * Time.deltaTime), BandLevels[i]);

                NormalizedBandLevels[i] = BandLevels[i] / BandPeaks[i];
                NormalizedBufferedBandLevels[i] = BufferedBandLevels[i] / BandPeaks[i];
            }
        }

        protected virtual void ComputeBandLevels()
        {
            float coefficient = Mathf.Log(RawSpectrum.Length);
            int offset = 0;
            for (int i = 0; i < BandLevels.Length; i++)
            {
                float savedSum = 0f;
                float next = Mathf.Exp(coefficient / BandLevels.Length * (i + 1));
                float weight = 1f / (next - offset);
                for (float sum = 0f; offset < next; offset++)
                {
                    sum += RawSpectrum[offset];
                    savedSum = sum;
                }
                BandLevels[i] = Mathf.Sqrt(weight * savedSum);
                if (BandLevels[i] > BandPeaks[i])
                {
                    BandPeaks[i] = BandLevels[i];
                    LastPeaksAt[i] = Time.time;
                }
            }
        }

        protected virtual void ComputeAmplitudes()
        {
            Amplitude = 0f;
            BufferedAmplitude = 0f;
            NormalizedAmplitude = 0f;
            NormalizedBufferedAmplitude = 0f;
            for (int i = 0; i < _cachedNumberOfBands; i++)
            {
                Amplitude += BandLevels[i];
                BufferedAmplitude += BufferedBandLevels[i];
                NormalizedAmplitude += NormalizedBandLevels[i];
                NormalizedBufferedAmplitude += NormalizedBufferedBandLevels[i];
            }
            Amplitude = Amplitude / _cachedNumberOfBands;
            BufferedAmplitude = BufferedAmplitude / _cachedNumberOfBands;
            NormalizedAmplitude = NormalizedAmplitude / _cachedNumberOfBands;
            NormalizedBufferedAmplitude = NormalizedBufferedAmplitude / _cachedNumberOfBands;
        }

        protected virtual void HandleBeats()
        {
            if (Beats.Length <= 0)
            {
                return;
            }

            foreach (Beat beat in Beats)
            {
                float value = 0f;
                beat.BeatThisFrame = false;
                switch (beat.Mode)
                {
                    case Beat.Modes.Amplitude:
                        value = Amplitude;
                        break;
                    case Beat.Modes.AmplitudeBuffered:
                        value = BufferedAmplitude;
                        break;
                    case Beat.Modes.BufferedNormalized:
                        value = NormalizedBufferedBandLevels[beat.BandID];
                        break;
                    case Beat.Modes.BufferedRaw:
                        value = BufferedBandLevels[beat.BandID];
                        break;
                    case Beat.Modes.Normalized:
                        value = NormalizedBandLevels[beat.BandID];
                        break;
                    case Beat.Modes.NormalizedAmplitude:
                        value = NormalizedAmplitude;
                        break;
                    case Beat.Modes.NormalizedAmplitudeBuffered:
                        value = NormalizedBufferedAmplitude;
                        break;
                    case Beat.Modes.Raw:
                        value = BandLevels[beat.BandID];
                        break;
                }

                if (beat.BeatValueMode == Beat.BeatValueModes.Live)
                {
                    beat.CurrentValue = value;
                }
                else
                {
                    // if audio value went below the bias during this frame
                    if ((beat._previousValue > beat.Threshold) && (value <= beat.Threshold))
                    {
                        // if minimum beat interval is reached
                        if (Time.time - beat._lastBeatAt > beat.MinimumTimeBetweenBeats)
                        {
                            OnBeat(beat, value);
                        }
                    }

                    // if audio value went above the bias during this frame
                    if ((beat._previousValue <= beat.Threshold) && (value > beat.Threshold))
                    {
                        // if minimum beat interval is reached
                        if (Time.time - beat._lastBeatAt > beat.MinimumTimeBetweenBeats)
                        {
                            OnBeat(beat, value);
                        }
                    }

                    beat._previousValue = value;
                }
            }
        }

        protected virtual void OnBeat(Beat beat, float rawValue)
        {
            beat._lastBeatAt = Time.time;
            beat.BeatThisFrame = true;
            if (beat.OnBeat != null)
            {
                beat.OnBeat.Invoke();
            }
            MMBeatEvent.Trigger(beat.Name, beat.CurrentValue);
            StartCoroutine(RemapBeat(beat));
        }

        protected virtual IEnumerator RemapBeat(Beat beat)
        {
            float remapStartedAt = Time.time;

            while (Time.time - remapStartedAt < beat.RemappedAttack + beat.RemappedDecay)
            {
                // attack
                if (Time.time - remapStartedAt < beat.RemappedAttack)
                {
                    beat.CurrentValue = Mathf.Lerp(0f, 1f, (Time.time - remapStartedAt) / beat.RemappedAttack);
                }
                if (Time.time - remapStartedAt > beat.RemappedAttack)
                {
                    beat.CurrentValue = Mathf.Lerp(1f, 0f, (Time.time - remapStartedAt - beat.RemappedAttack) / beat.RemappedDecay);
                }
                yield return null;
            }
            beat.CurrentValue = 0f;
            yield break;
        }

        protected virtual void OnValidate()
        {
            if ((Beats == null) || (Beats.Length == 0))
            {
                return;
            }

            int bandCounter = 0;
            for (int i = 0; i < Beats.Length; i++)
            {
                if (bandCounter >= _cachedNumberOfBands)
                {
                    bandCounter = 0;
                }
                Beats[i].InitializeIfNeeded(i, bandCounter);
                bandCounter++;
            }
        }
    }
}