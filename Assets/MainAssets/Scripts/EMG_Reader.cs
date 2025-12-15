using UnityEngine;

using System.IO.Ports;

using System.Threading;

using System;
using UnityEngine.UI;

public class EMG_Reader : MonoBehaviour
{
    public string comPort = "COM5";
    public int baudRate = 115200;
    public volatile float emgValue = 0f;
    private SerialPort stream;
    private Thread readThread;
    private volatile bool keepReading = false;

    //Optional, cube vis
    // public GameObject chest;

    // smoothing, can use other method
    private float displayedValue = 0f;

    void Start()
{
    DontDestroyOnLoad(gameObject);  // Prevent destruction on scene changes
    Debug.Log("[EMG] Start() called - keeping GameObject alive");
    OpenPort();
}

    void OpenPort()
    {
        try
        {
            stream = new SerialPort(comPort, baudRate);
            stream.ReadTimeout = 100;
            stream.Open();
            Debug.Log($"[EMG] Opened {comPort} @ {baudRate}");
            keepReading = true;
            readThread = new Thread(ReadLoop);
            readThread.IsBackground = true;
            readThread.Start();
        }

        catch (System.Exception e)
        {
            Debug.LogError($"[EMG] Failed to open {comPort}: {e.Message}");
            stream = null;
        }

    }

    void ReadLoop()
    {
        while (keepReading)
        {
            try
            {
                string line = stream.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    if (float.TryParse(line.Trim(), out float v))
                    {
                        emgValue = Mathf.Abs(v);
                    }
                }
            }

            catch (TimeoutException)
            {
                // Ignore timeout exceptions
                Debug.Log("Timeout");
            }

            catch (System.Exception e)
            {
                Debug.LogWarning($"[EMG] Read error: {e.Message}");
            }

            Thread.Sleep(1); // small yield

        }
    }

    void Update()
    {
        // 1. 先一樣做平滑（避免顏色抖來抖去）
        Debug.Log("UpdatingＥＭＧ");
        displayedValue = Mathf.Lerp(displayedValue, emgValue, 0.15f);

        // if (chest != null)
        // {
            // 2. 把 displayedValue 正規化成 0~1
            //    把 maxEmg 改小一點，讓變化更敏感
            float maxEmg = 300f;  // 先試 120，如果還是不夠敏感再往下調 80、100 都可以
            float t = displayedValue / maxEmg;
            t = Mathf.Clamp01(t);   // 限制在 0~1 之間

            // 3. 用 gamma 把「小力量」放大（gamma < 1）
            //    t 原本線性：0.2 -> 0.2
            //    開根號後     ：sqrt(0.2) ≈ 0.45  → 視覺上會亮很多
            float gamma = 0.3f;        // = 開根號
            t = Mathf.Pow(t, gamma);

            // 4. 可以給一點底亮度，看起來比較不死
            float minIntensity = 0.15f;             // 放鬆時的暗紅程度
            float intensity = Mathf.Lerp(minIntensity, 1.0f, t);
            SharedInfoManager.Instance.SetEMGSignal(intensity);
            // // Color c = new Color(intensity, 0f, 0f);

            // RawImage musle = chest.GetComponent<RawImage>();
            // Color color_a = musle.color;
            // if (color_a != null)
            // {
            //     color_a.a = intensity;
            //     // print(Color.a);
            // }
        // }

        // 暫時開一下看數值範圍，之後覺得 OK 就關掉
        Debug.Log($"EMG raw: {emgValue:F2}  disp: {displayedValue:F2}");
    }



    void OnApplicationQuit()
    {
        ClosePort();
    }

    void OnDisable()
{
    Debug.Log($"[EMG] OnDisable called! GameObject active: {gameObject.activeInHierarchy}");
    Debug.LogError(System.Environment.StackTrace); // Show what called this
    ClosePort();
}
    void ClosePort()
    {
        keepReading = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(200);
        }

        if (stream != null)
        {
            try
            {
                if (stream.IsOpen) stream.Close();
                stream.Dispose();
            }

            catch (System.Exception e)
            {
                Debug.LogWarning("[EMG] Error closing port: " + e.Message);
            }

            stream = null;
        }

        Debug.Log("[EMG] Port closed.");
    }
}