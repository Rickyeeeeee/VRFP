using UnityEngine;

public class SharedInfoManager : MonoBehaviour
{
    public static SharedInfoManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        barUpperRefPosition = new Vector3(0.0f, 1.26f, 0.5f) + new Vector3(0.0f, 0.15f, 0.0f);
        barLowerRefPosition = new Vector3(0.0f, 1.26f, 0.5f) + new Vector3(0.0f, -0.45f, 0.0f);
        barRefRotation = new Vector3(0.0f, 90.0f, 0.0f);
    }

    void Update()
    {
    }

    // Triggers
    [SerializeField] private bool isOkayRickyDetected = false;
    [SerializeField] private bool isLying = false;
    [SerializeField] private bool isHandPositionCorrect = false;

    public bool GetIsOkayRickyDetected() => isOkayRickyDetected;
    public void SetIsOkayRickyDetected(bool value) => isOkayRickyDetected = value;
    public bool GetIsLying() => isLying;
    public void SetIsLying(bool value) => isLying = value;
    public bool GetIsHandPositionCorrect() => isHandPositionCorrect;
    public void SetIsHandPositionCorrect(bool value) => isHandPositionCorrect = value;

    public void ResetAllTriggers()
    {
        isOkayRickyDetected = false;
        isLying = false;
        isHandPositionCorrect = false;
    }

    // Train Couroutine 1, 2, 3
    public GameObject bar;
    private Vector3 barUpperRefPosition;
    private Vector3 barLowerRefPosition;
    private Vector3 barRefRotation;

    public Vector3 GetBarCurrentPosition() => bar.transform.position;
    public void SetBarCurrentPosition(Vector3 value) => bar.transform.position = value;
    public Vector3 GetBarCurrentRotation() => bar.transform.eulerAngles;
    public void SetBarCurrentRotation(Vector3 value) => bar.transform.eulerAngles = value;
    public Vector3 GetBarUpperRefPosition() => barUpperRefPosition;
    public void SetBarUpperRefPosition(Vector3 value) => barUpperRefPosition = value;
    public Vector3 GetBarLowerRefPosition() => barLowerRefPosition;
    public void SetBarLowerRefPosition(Vector3 value) => barLowerRefPosition = value;
    public Vector3 GetBarRefRotation() => barRefRotation;
    public void SetBarRefRotation(Vector3 value) => barRefRotation = value;

    // Train Couroutine 4
    [SerializeField, Range(0f, 1f)] private float EMGSignal;

    public float GetEMGSignal() => EMGSignal;
    public void SetEMGSignal(float value) => EMGSignal = value;

    // Train Couroutine 5
    [SerializeField] private float pressSignalAtLowerPoint;
    [SerializeField] private float pressSignalAtHigherPoint;

    public float GetPressSignalAtLowerPoint() => pressSignalAtLowerPoint;
    public void SetPressSignalAtLowerPoint(float value) => pressSignalAtLowerPoint = value;
    public float GetPressSignalAtHigherPoint() => pressSignalAtHigherPoint;
    public void SetPressSignalAtHigherPoint(float value) => pressSignalAtHigherPoint = value;

    // Train Couroutine 5 - Left and Right separated
    [SerializeField] private float leftPressSignalAtLowerPoint;
    [SerializeField] private float leftPressSignalAtHigherPoint;
    [SerializeField] private float rightPressSignalAtLowerPoint;
    [SerializeField] private float rightPressSignalAtHigherPoint;

    public float GetLeftPressSignalAtLowerPoint() => leftPressSignalAtLowerPoint;
    public void SetLeftPressSignalAtLowerPoint(float value) => leftPressSignalAtLowerPoint = value;
    public float GetLeftPressSignalAtHigherPoint() => leftPressSignalAtHigherPoint;
    public void SetLeftPressSignalAtHigherPoint(float value) => leftPressSignalAtHigherPoint = value;
    public float GetRightPressSignalAtLowerPoint() => rightPressSignalAtLowerPoint;
    public void SetRightPressSignalAtLowerPoint(float value) => rightPressSignalAtLowerPoint = value;
    public float GetRightPressSignalAtHigherPoint() => rightPressSignalAtHigherPoint;
    public void SetRightPressSignalAtHigherPoint(float value) => rightPressSignalAtHigherPoint = value;
}
