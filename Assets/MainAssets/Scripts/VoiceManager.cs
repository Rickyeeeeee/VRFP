using System.Collections;
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
    [Tooltip("Recording duration (s)")]
    public float recordingDuration = 2f;
    [Tooltip("Volume threshold")]
    public float volumeThreshold = 0.0001f;
    [Tooltip("Checking Interval (s)")]
    public float checkInterval = 0.5f;
    
    private AudioClip _clip;
    private string _micDevice;
    private bool _isProcessing = false;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            _micDevice = Microphone.devices[0];
            Debug.LogWarning("[INFO] Microphone device found: " + _micDevice);
            StartCoroutine(SmartListening());
        }
        else
        {
            Debug.LogError("[ERROR] Cannot find microphone device!");
        }
    }

    IEnumerator SmartListening()
    {
        while (true)
        {
            if (!_isProcessing)
            {
                _clip = Microphone.Start(_micDevice, false, (int)recordingDuration + 1, 16000);
             
                yield return new WaitForSeconds(recordingDuration);

                float volume = GetAverageVolume();
                // Debug.LogWarning($"[INFO] Average Volume: {volume}");
                
                if (volume > volumeThreshold)
                {
                    Debug.LogWarning($"[INFO] Voice detected! Volume: {volume}");
                    StopAndTranscribe();
                }
                else
                {
                    Microphone.End(_micDevice);
                }
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
    }

    float GetAverageVolume()
    {
        float[] data = new float[_clip.samples];
        _clip.GetData(data, 0);
        
        float sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += Mathf.Abs(data[i]);
        }
        
        return sum / data.Length;
    }

    async void StopAndTranscribe()
    {
        if (_micDevice == null) return;

        _isProcessing = true;
        Microphone.End(_micDevice);

        var res = await whisper.GetTextAsync(_clip);
        string result = res.Result;
        if(outputText) outputText.text = result;
        
        Debug.LogWarning("[INFO] Detection:" + result);

        ProcessCommand(result);
        _isProcessing = false;
    }


    void ProcessCommand(string text)
    {
        text = text.ToLower();

        if (text.Contains("ok ricky") || text.Contains("okay ricky") || text.Contains("ok, ricky") || text.Contains("okay, ricky"))
        {
            Debug.LogWarning("[INFO] Command: OK Ricky");
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