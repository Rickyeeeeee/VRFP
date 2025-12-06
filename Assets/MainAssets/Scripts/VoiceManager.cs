using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whisper;
using UnityEngine.UI; 
using TMPro; 

public class VoiceManager : MonoBehaviour
{
    public WhisperManager whisper; 
    public TextMeshProUGUI outputText;      
    // public VoiceCommands voiceCommands;

    [Header("Recording Settings")]
    // [Tooltip("Recording duration (s)")]
    // public float recordingDuration = 2f;
    // [Tooltip("Volume threshold")]
    // public float volumeThreshold = 0.0005f;
    // [Tooltip("Checking Interval (s)")]
    // public float checkInterval = 0.1f;
    [Tooltip("Sensitivity")]
    public float vadThreshold = 0.01f;  // Higher -> less sensitive
    [Tooltip("Max recording length (s)")]
    public int maxRecordingLength = 2; 
    [Tooltip("How long silence to wait before stopping recording (s)")]
    public float silenceDurationToStop = 0.5f;
    
    private string _micDevice;
    private bool _isProcessing = false;
    private bool _isRecording = false;
    
    // VAD setting
    private AudioClip _clip;
    private int _sampleRate = 16000;
    private int _lastSamplePos = 0;
    private float _silenceTimer = 0f;
    private List<float> _accumulatedSamples = new List<float>();

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            _micDevice = Microphone.devices[0];
            Debug.LogWarning("[INFO] Microphone device found: " + _micDevice);
            StartMicrophone();
        }
        else
        {
            Debug.LogError("[ERROR] Cannot find microphone device!");
        }
    }

    void StartMicrophone()
    {
        _clip = Microphone.Start(_micDevice, true, maxRecordingLength, _sampleRate);
        _lastSamplePos = 0;
    }

    void Update()
    {
        if (_isProcessing || _micDevice == null) return;

        int currentPos = Microphone.GetPosition(_micDevice);
        if (currentPos < 0 || currentPos == _lastSamplePos) return;

        // Calculate how many samples to read
        int samplesToRead = currentPos - _lastSamplePos;
        if (samplesToRead < 0) samplesToRead += _clip.samples;

        float[] waveData = new float[samplesToRead];

        if (_lastSamplePos + samplesToRead <= _clip.samples)
        {
            _clip.GetData(waveData, _lastSamplePos);
        }
        else
        {
            int endPart = _clip.samples - _lastSamplePos;
            float[] part1 = new float[endPart];
            float[] part2 = new float[samplesToRead - endPart];
            _clip.GetData(part1, _lastSamplePos);
            _clip.GetData(part2, 0);
            System.Array.Copy(part1, 0, waveData, 0, endPart);
            System.Array.Copy(part2, 0, waveData, endPart, part2.Length);
        }

        _lastSamplePos = currentPos;

        float maxVolume = 0f;
        foreach (var s in waveData)
        {
            if (Mathf.Abs(s) > maxVolume) maxVolume = Mathf.Abs(s);
        }

        // VAD logic
        if (maxVolume > vadThreshold)
        {
            // Voice detected
            if (!_isRecording)
            {
                // Debug.Log("[INFO] Voice started...");
                _isRecording = true;
                _accumulatedSamples.Clear();
            }
            _silenceTimer = 0f; // Reset silence timer
        }
        else if (_isRecording)
        {
            // Currently recording, but silence detected
            _silenceTimer += Time.deltaTime; // Note: This is approximate based on frame rate
            // Better: _silenceTimer += (float)samplesToRead / _sampleRate;
        }

        // If we are recording, accumulate samples
        if (_isRecording)
        {
            _accumulatedSamples.AddRange(waveData);

            // Check if we should stop
            // 1. Silence for too long
            // 2. Buffer too big (safety cap)
            if (_silenceTimer > silenceDurationToStop || _accumulatedSamples.Count > _sampleRate * maxRecordingLength)
            {
                // Debug.Log("[INFO] Voice ended. Transcribing...");
                StopAndTranscribe();
            }
        }
    }


    async void StopAndTranscribe()
    {
        _isRecording = false;
        _isProcessing = true;
        _silenceTimer = 0f;

        // Create a temporary clip from accumulated data
        if (_accumulatedSamples.Count > 0)
        {
            float[] finalData = _accumulatedSamples.ToArray();
            AudioClip clipToTranscribe = AudioClip.Create("TempVoice", finalData.Length, 1, _sampleRate, false);
            clipToTranscribe.SetData(finalData, 0);

            var res = await whisper.GetTextAsync(clipToTranscribe);
            string result = res.Result;
            if(outputText) outputText.text = result;
            
            Debug.LogWarning("[INFO] Detection:" + result);
            ProcessCommand(result);
            
            // Clean up
            Destroy(clipToTranscribe);
        }

        _accumulatedSamples.Clear();
        _isProcessing = false;
    }


    void ProcessCommand(string text)
    {
        text = text.ToLower();

        if (text.Contains("ok ricky") || text.Contains("okay ricky") || text.Contains("ok, ricky") || text.Contains("okay, ricky"))
        {
            // Debug.LogWarning("[INFO] Command: OK Ricky");
            // voiceCommands.ChangeCubeColor();
            if (SharedInfoManager.Instance != null)
            {
                SharedInfoManager.Instance.SetIsOkayRickyDetected(true);
            }
            else
            {
                Debug.LogError("[ERROR] SharedInfoManager instance not found!");
            }
        }
        else if (text.Contains("passthrough mode") || text.Contains("pass through mode"))
        {
            Debug.Log("[INFO] Command: Passthrough Mode");
        }
        else if (text.Contains("vr mode") || text.Contains("v are mode"))
        {
            Debug.Log("[INFO] Command: VR Mode");
        }
        else
        {
            Debug.Log("[INFO] Undefined command");
        }
    }
}