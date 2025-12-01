using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    // Instruction
    public AudioSource welcomeAudioSource;
    public AudioSource setupDevicesAudioSource;
    public AudioSource adjustSettingsAudioSource;
    public AudioSource lieDownAudioSource;
    public AudioSource recordGripWidthAudioSource;
    public AudioSource detectHandsAudioSource;
    public AudioSource enterVRModeAudioSource;

    public void StopAllInstructions()
    {
        welcomeAudioSource.Stop();
        setupDevicesAudioSource.Stop();
        adjustSettingsAudioSource.Stop();
        lieDownAudioSource.Stop();
        recordGripWidthAudioSource.Stop();
        detectHandsAudioSource.Stop();
        enterVRModeAudioSource.Stop();
    }
}
