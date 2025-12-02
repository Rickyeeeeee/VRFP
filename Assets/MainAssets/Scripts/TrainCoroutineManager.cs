using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainCoroutineManager : MonoBehaviour
{
    public static TrainCoroutineManager Instance { get; private set; }

    private static readonly Color CORRECT_COLOR = new Color(0.28f, 0.5f, 0.28f, 1f);
    private static readonly Color WRONG_COLOR = new Color(0.5f, 0.2f, 0.2f, 1f);

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

    // Train Coroutines
    private Coroutine trainCoroutine1;  // bar position and rotation
    private Coroutine trainCoroutine2;  // height
    private Coroutine trainCoroutine3;  // tilt
    private Coroutine trainCoroutine4;  // emg
    private Coroutine trainCoroutine5;  // press
    private Coroutine trainCoroutine5_5;  // temporary: press based on rotation (until pressure sensor available)

    public void StartAllTrainCoroutines()
    {
        trainCoroutine1 = StartCoroutine(TrainCoroutine1());
        trainCoroutine2 = StartCoroutine(TrainCoroutine2());
        trainCoroutine3 = StartCoroutine(TrainCoroutine3());
        trainCoroutine4 = StartCoroutine(TrainCoroutine4());
        trainCoroutine5 = StartCoroutine(TrainCoroutine5());
        trainCoroutine5_5 = StartCoroutine(TrainCoroutine5_5());
    }

    private IEnumerator TrainCoroutine1()
    {
        float startingZRotation = 0f;
        bool startingZCaptured = false;
        
        while (true)
        {
            Vector3 currentPosition = SharedInfoManager.Instance.GetBarCurrentPosition();
            Vector3 currentRotation = SharedInfoManager.Instance.GetBarCurrentRotation();
            Vector3 initialPosition = SharedInfoManager.Instance.GetBarUpperRefPosition();
            Vector3 initialRotation = SharedInfoManager.Instance.GetBarRefRotation();

            if (!startingZCaptured)
            {
                startingZRotation = currentRotation.z;
                startingZCaptured = true;
            }

            Vector3 positionDifference = Quaternion.Inverse(Quaternion.Euler(initialRotation)) * (currentPosition - initialPosition);
            Vector3 rotationDifference = currentRotation - initialRotation;

            float scale = 100.0f;
            Vector3 scaledpositionDifference = positionDifference * scale;
            UIManager.Instance.bar.transform.localPosition = new Vector3(scaledpositionDifference.z, scaledpositionDifference.x, 0.0f);
            UIManager.Instance.redSpot.transform.localPosition = new Vector3(scaledpositionDifference.z, scaledpositionDifference.x, 0.0f);

            UIManager.Instance.bar.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationDifference.y);
            UIManager.Instance.redSpot.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationDifference.y);
            Debug.Log("positionDifference: " + positionDifference);
            Debug.Log("rotationDifference: " + rotationDifference);
            bool isAllCorrect = true;
            
            float positionThreshold = 0.01f; 
            bool isBarCenteredX = Mathf.Abs(positionDifference.x) < positionThreshold;
            bool isBarCenteredZ = Mathf.Abs(positionDifference.z) < positionThreshold;
            bool isBarCentered = isBarCenteredX && isBarCenteredZ;
            if (!isBarCentered) isAllCorrect = false;
            
            float zRotationThreshold = 30.0f;
            float currentZ = currentRotation.z;
            float initialZ = startingZRotation;
            
            currentZ = currentZ % 360f;
            if (currentZ < 0f) currentZ += 360f;
            initialZ = initialZ % 360f;
            if (initialZ < 0f) initialZ += 360f;
            
            // float zRotationDiff = currentZ - initialZ;
            // if (zRotationDiff > 180f) zRotationDiff -= 360f;
            // if (zRotationDiff < -180f) zRotationDiff += 360f;
            
            // bool isZRotationCorrect = Mathf.Abs(zRotationDiff) <= zRotationThreshold;
            // if (!isZRotationCorrect) isAllCorrect = false;
            
            float xAngleThreshold = 5.0f;
            bool isTiltCorrect = (360.0f - xAngleThreshold <= rotationDifference.x || rotationDifference.x <= xAngleThreshold);
            if (!isTiltCorrect) isAllCorrect = false;
            
            float yRotationThreshold = 5.0f; 
            float yRotationDiff = rotationDifference.y;
            if (yRotationDiff > 180f) yRotationDiff -= 360f;
            if (yRotationDiff < -180f) yRotationDiff += 360f;
            bool isYRotationCorrect = Mathf.Abs(yRotationDiff) <= yRotationThreshold;
            if (!isYRotationCorrect) isAllCorrect = false;
            
            if (UIManager.Instance.overallPanelBackground != null)
            {
                RawImage overallBgImage = UIManager.Instance.overallPanelBackground.GetComponent<RawImage>();
                overallBgImage.color = isAllCorrect ? CORRECT_COLOR : WRONG_COLOR;
            }

            yield return null;
        }
    }

    private IEnumerator TrainCoroutine2()
    {
        bool isGoingDown;
        int repCount = 0;

        isGoingDown = true;
        VisualizationManager.Instance.upperIndicator.SetActive(false);
        VisualizationManager.Instance.lowerIndicator.SetActive(true);

        while (true)
        {
            Vector3 currentPosition = SharedInfoManager.Instance.GetBarCurrentPosition();
            Vector3 objectivePosition = isGoingDown ? SharedInfoManager.Instance.GetBarLowerRefPosition() : SharedInfoManager.Instance.GetBarUpperRefPosition();

            bool isSatisfied = isGoingDown && currentPosition.y <= objectivePosition.y || !isGoingDown && currentPosition.y >= objectivePosition.y;

            if (isSatisfied)
            {
                bool wasGoingDown = isGoingDown;
                isGoingDown = !isGoingDown;
                
                if (!wasGoingDown && isGoingDown)
                {
                    repCount++;
                    if (UIManager.Instance.repCountText != null)
                    {
                        UIManager.Instance.repCountText.text = repCount.ToString();
                    }
                }
                
                if (isGoingDown)
                {
                    VisualizationManager.Instance.upperIndicator.SetActive(false);
                    VisualizationManager.Instance.lowerIndicator.SetActive(true);
                }
                else
                {
                    VisualizationManager.Instance.upperIndicator.SetActive(true);
                    VisualizationManager.Instance.lowerIndicator.SetActive(false);
                }
            }

            yield return null;
        }
    }

    private IEnumerator TrainCoroutine3()
    {
        while (true)
        {
            Vector3 currentPosition = SharedInfoManager.Instance.GetBarCurrentPosition();
            Vector3 currentRotation = SharedInfoManager.Instance.GetBarCurrentRotation();
            Vector3 initialRotation = SharedInfoManager.Instance.GetBarRefRotation();

            Vector3 rotationDifference = currentRotation - initialRotation;

            float angleThreshold = 5.0f;
            if (360.0f - angleThreshold <= rotationDifference.x || rotationDifference.x <= angleThreshold)
            {
                VisualizationManager.Instance.leftIndicator.SetActive(false);
                VisualizationManager.Instance.rightIndicator.SetActive(false);
            }
            else
            {
                if (rotationDifference.x < 180.0f)
                {
                    VisualizationManager.Instance.leftIndicator.SetActive(false);
                    VisualizationManager.Instance.rightIndicator.SetActive(true);
                }
                else
                {
                    VisualizationManager.Instance.leftIndicator.SetActive(true);
                    VisualizationManager.Instance.rightIndicator.SetActive(false);
                }
            }

            VisualizationManager.Instance.barRotationIndicators.transform.position = currentPosition;
            VisualizationManager.Instance.barRotationIndicators.transform.eulerAngles = currentRotation;
            VisualizationManager.Instance.leftIndicator.transform.eulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
            VisualizationManager.Instance.rightIndicator.transform.eulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);

            yield return null;
        }
    }

    private IEnumerator TrainCoroutine4()
    {
        RawImage rawImage = UIManager.Instance.chestMuscle.GetComponent<RawImage>();

        while (true)
        {
            float emg = SharedInfoManager.Instance.GetEMGSignal();
            float value = Mathf.Lerp(0.2f, 1.0f, emg);
            Color color = rawImage.color;
            color.a = value;
            rawImage.color = color;

            yield return null;
        }
    }

    private IEnumerator TrainCoroutine5()
    {
        
        float wrongPositionThreshold = 0.6f; 
        
        while (true)
        {
            // Get left hand signals
            float leftLower = SharedInfoManager.Instance.GetLeftPressSignalAtLowerPoint();
            float leftHigher = SharedInfoManager.Instance.GetLeftPressSignalAtHigherPoint();
            float leftSum = leftLower + leftHigher;
            
            // Get right hand signals
            float rightLower = SharedInfoManager.Instance.GetRightPressSignalAtLowerPoint();
            float rightHigher = SharedInfoManager.Instance.GetRightPressSignalAtHigherPoint();
            float rightSum = rightLower + rightHigher;

            bool isLeftWrong = false;
            bool isRightWrong = false;

            float leftHigherPercentage = leftHigher / leftSum;
            isLeftWrong = leftHigherPercentage >= wrongPositionThreshold;
            UIManager.Instance.leftHandWrongImage.SetActive(isLeftWrong);
            //Left hand
            RawImage leftHandCorrectImage = UIManager.Instance.leftHandCorrectImage.GetComponent<RawImage>();
            Color leftHandCorrectColor = leftHandCorrectImage.color;
            if (isLeftWrong)
            {    
                leftHandCorrectColor.a = 0.6f;    
            }
            else{
                leftHandCorrectColor.a = 1.0f;
            }
            leftHandCorrectImage.color = leftHandCorrectColor;
            
            
            float rightHigherPercentage = rightHigher / rightSum;
            isRightWrong = rightHigherPercentage >= wrongPositionThreshold;
            UIManager.Instance.rightHandWrongImage.SetActive(isRightWrong);
            //Right hand
            RawImage rightHandCorrectImage = UIManager.Instance.rightHandCorrectImage.GetComponent<RawImage>();
            Color rightHandCorrectColor = rightHandCorrectImage.color;
            if (isRightWrong)
            {
                rightHandCorrectColor.a = 0.6f;
            }
            else{
                rightHandCorrectColor.a = 1.0f;
            }
            rightHandCorrectImage.color = rightHandCorrectColor;
            if (UIManager.Instance.leftHandPanelBackground != null)
            {
                RawImage leftBgImage = UIManager.Instance.leftHandPanelBackground.GetComponent<RawImage>();
                leftBgImage.color = isLeftWrong ? WRONG_COLOR : CORRECT_COLOR;
            }
            if (UIManager.Instance.rightHandPanelBackground != null)
            {
                RawImage rightBgImage = UIManager.Instance.rightHandPanelBackground.GetComponent<RawImage>();
                rightBgImage.color = isRightWrong ? WRONG_COLOR : CORRECT_COLOR;
            }
            
            yield return null;
        }
    }

    private IEnumerator TrainCoroutine5_5()
    {
        float rotationThreshold = 30.0f;
        float startingZRotation = SharedInfoManager.Instance.GetBarCurrentRotation().z;
        bool startingZCaptured = false;
        
        while (true)
        {
            
            Vector3 currentRotation = SharedInfoManager.Instance.GetBarCurrentRotation();
            
            if (!startingZCaptured)
            {
                startingZRotation = currentRotation.z;
                startingZCaptured = true;
            }
            
            float currentZ = currentRotation.z;
            float initialZ = startingZRotation;
            Debug.Log(currentRotation);
            currentZ = currentZ % 360f;
            if (currentZ < 0f) currentZ += 360f;
            initialZ = initialZ % 360f;
            if (initialZ < 0f) initialZ += 360f;
            
            float zRotationDiff = currentZ - initialZ;
            if (zRotationDiff > 180f) zRotationDiff -= 360f;
            if (zRotationDiff < -180f) zRotationDiff += 360f;

            bool isWrong = Mathf.Abs(zRotationDiff) > rotationThreshold;
            bool isLeftWrong = isWrong;
            bool isRightWrong = isWrong;
        
            UIManager.Instance.leftHandWrongImage.SetActive(isLeftWrong);
            RawImage leftHandCorrectImage = UIManager.Instance.leftHandCorrectImage.GetComponent<RawImage>();
            Color leftHandCorrectColor = leftHandCorrectImage.color;
            leftHandCorrectColor.a = isLeftWrong ? 0.6f : 1.0f;
            leftHandCorrectImage.color = leftHandCorrectColor;
            
            UIManager.Instance.rightHandWrongImage.SetActive(isRightWrong);
            RawImage rightHandCorrectImage = UIManager.Instance.rightHandCorrectImage.GetComponent<RawImage>();
            Color rightHandCorrectColor = rightHandCorrectImage.color;
            rightHandCorrectColor.a = isRightWrong ? 0.6f : 1.0f;
            rightHandCorrectImage.color = rightHandCorrectColor;
        
            if (UIManager.Instance.leftHandPanelBackground != null)
            {
                RawImage leftBgImage = UIManager.Instance.leftHandPanelBackground.GetComponent<RawImage>();
                leftBgImage.color = isLeftWrong ? WRONG_COLOR : CORRECT_COLOR;
            }
            if (UIManager.Instance.rightHandPanelBackground != null)
            {
                RawImage rightBgImage = UIManager.Instance.rightHandPanelBackground.GetComponent<RawImage>();
                rightBgImage.color = isRightWrong ? WRONG_COLOR : CORRECT_COLOR;
            }
            
            yield return null;
        }
    }

    public void StopAllTrainCoroutines()
    {
        StopTrainCoroutine(trainCoroutine1);
        StopTrainCoroutine(trainCoroutine2);
        StopTrainCoroutine(trainCoroutine3);
        StopTrainCoroutine(trainCoroutine4);
        StopTrainCoroutine(trainCoroutine5);
        StopTrainCoroutine(trainCoroutine5_5);
    }

    private void StopTrainCoroutine(Coroutine trainCoroutine)
    {
        if (trainCoroutine != null)
        {
            StopCoroutine(trainCoroutine);
            trainCoroutine = null;
        }
    }
}
