using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainCoroutineManager : MonoBehaviour
{
    public static TrainCoroutineManager Instance { get; private set; }

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

    public void StartAllTrainCoroutines()
    {
        trainCoroutine1 = StartCoroutine(TrainCoroutine1());
        trainCoroutine2 = StartCoroutine(TrainCoroutine2());
        trainCoroutine3 = StartCoroutine(TrainCoroutine3());
        trainCoroutine4 = StartCoroutine(TrainCoroutine4());
        trainCoroutine5 = StartCoroutine(TrainCoroutine5());
    }

    private IEnumerator TrainCoroutine1()
    {
        while (true)
        {
            Vector3 currentPosition = SharedInfoManager.Instance.GetBarCurrentPosition();
            Vector3 currentRotation = SharedInfoManager.Instance.GetBarCurrentRotation();
            Vector3 initialPosition = SharedInfoManager.Instance.GetBarUpperRefPosition();
            Vector3 initialRotation = SharedInfoManager.Instance.GetBarRefRotation();

            Vector3 positionDifference = Quaternion.Inverse(Quaternion.Euler(initialRotation)) * (currentPosition - initialPosition);
            Vector3 rotationDifference = currentRotation - initialRotation;

            float scale = 100.0f;
            Vector3 scaledpositionDifference = positionDifference * scale;
            UIManager.Instance.bar.transform.localPosition = new Vector3(scaledpositionDifference.z, scaledpositionDifference.x, 0.0f);
            UIManager.Instance.redSpot.transform.localPosition = new Vector3(scaledpositionDifference.z, scaledpositionDifference.x, 0.0f);

            UIManager.Instance.bar.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationDifference.y);
            UIManager.Instance.redSpot.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationDifference.y);

            yield return null;
        }
    }

    private IEnumerator TrainCoroutine2()
    {
        bool isGoingDown;

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
                isGoingDown = !isGoingDown;
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
            float value = Mathf.Lerp(0.2f, 0.8f, emg);
            Color color = rawImage.color;
            color.a = value;
            rawImage.color = color;

            yield return null;
        }
    }

    private IEnumerator TrainCoroutine5()
    {
        while (true)
        {
            float pressSignalAtLowerPoint = SharedInfoManager.Instance.GetPressSignalAtLowerPoint();
            float pressSignalAtHigherPoint = SharedInfoManager.Instance.GetPressSignalAtHigherPoint();

            float sum = pressSignalAtLowerPoint + pressSignalAtHigherPoint;
            
            if (pressSignalAtLowerPoint < 0.0f || pressSignalAtHigherPoint < 0.0f || sum <= 0.0f)
            {
                UIManager.Instance.pressSignalAtLowerPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, 0.0f);
                UIManager.Instance.pressSignalAtLowerPointText.GetComponent<TextMeshProUGUI>().text = "";
                UIManager.Instance.pressSignalAtHigherPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, 0.0f);
                UIManager.Instance.pressSignalAtHigherPointText.GetComponent<TextMeshProUGUI>().text = "";
                yield return null;
                continue;
            }

            int percentageAtLowerPoint = Mathf.RoundToInt(pressSignalAtLowerPoint / sum * 100f);
            int percentageAtHigherPoint = 100 - percentageAtLowerPoint;

            float scale = 10.0f;
            float radiusAtLowerPoint = percentageAtLowerPoint / scale;
            float radiusAtHigherPoint = percentageAtHigherPoint / scale;

            UIManager.Instance.pressSignalAtLowerPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(radiusAtLowerPoint, radiusAtLowerPoint);
            UIManager.Instance.pressSignalAtLowerPointText.GetComponent<TextMeshProUGUI>().text = percentageAtLowerPoint.ToString() + "%";
            UIManager.Instance.pressSignalAtHigherPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(radiusAtHigherPoint, radiusAtHigherPoint);
            UIManager.Instance.pressSignalAtHigherPointText.GetComponent<TextMeshProUGUI>().text = percentageAtHigherPoint.ToString() + "%";

            yield return null;
        }
    }

    public void StopAllTrainCoroutines()
    {
        StopTrainCoroutine(trainCoroutine1);
        StopTrainCoroutine(trainCoroutine2);
        StopTrainCoroutine(trainCoroutine3);
        StopTrainCoroutine(trainCoroutine4);
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
