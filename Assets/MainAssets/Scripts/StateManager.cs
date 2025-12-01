using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //ChangeStateToLandingPage();
        ChangeStateToTrain();
        //ChangeStateToRecordGripWidth();
    }

    void Update()
    {

    }

    // State
    public enum State
    {
        Null,  // unused

        LandingPage,  // unused
        Welcome,
        SetupDevices,
        AdjustSettings,
        LieDown,
        RecordGripWidth,
        DetectHands,
        EnterVRMode,

        Train
    };

    private State currentState;
    private Coroutine checkChangeStateCoroutine;

    public State GetCurrentState() => currentState;
    public void ChangeStateToNull() => ChangeState(State.Null);
    public void ChangeStateToLandingPage() => ChangeState(State.LandingPage);
    public void ChangeStateToWelcome() => ChangeState(State.Welcome);
    public void ChangeStateToSetupDevices() => ChangeState(State.SetupDevices);
    public void ChangeStateToAdjustSettings() => ChangeState(State.AdjustSettings);
    public void ChangeStateToLieDown() => ChangeState(State.LieDown);
    public void ChangeStateToRecordGripWidth() => ChangeState(State.RecordGripWidth);
    public void ChangeStateToDetectHands() => ChangeState(State.DetectHands);
    public void ChangeStateToEnterVRMode() => ChangeState(State.EnterVRMode);
    public void ChangeStateToTrain() => ChangeState(State.Train);

    private void ChangeState(State newState)
    {
        if (newState == currentState)
            return;

        currentState = newState;
        Debug.Log($"Change to state: {currentState}");

        if (checkChangeStateCoroutine != null)
        {
            StopCoroutine(checkChangeStateCoroutine);
            checkChangeStateCoroutine = null;
        }

        UIManager.Instance.HideAllPages();
        AudioManager.Instance.StopAllInstructions();
        VisualizationManager.Instance.HideAllVisualization();
        TrainCoroutineManager.Instance.StopAllTrainCoroutines();
        SharedInfoManager.Instance.ResetAllTriggers();

        switch (currentState)
        {
            case State.Null:
                break;

            case State.LandingPage:
                //UIManager.Instance.landingPage.SetActive(true);
                break;

            case State.Welcome:
                UIManager.Instance.welcomePage.SetActive(true);
                AudioManager.Instance.welcomeAudioSource.Play();
                checkChangeStateCoroutine = StartCoroutine(
                    NotChangeStateUntil(() => !AudioManager.Instance.welcomeAudioSource.isPlaying, State.SetupDevices)
                );
                break;

            case State.SetupDevices:
                UIManager.Instance.setupDevicesPage.SetActive(true);
                AudioManager.Instance.setupDevicesAudioSource.Play();
                checkChangeStateCoroutine = StartCoroutine(
                    NotChangeStateUntil(() => SharedInfoManager.Instance.GetIsOkayRickyDetected(), State.AdjustSettings)
                );
                break;

            case State.AdjustSettings:
                UIManager.Instance.adjustSettingsPage.SetActive(true);
                AudioManager.Instance.adjustSettingsAudioSource.Play();
                break;

            case State.LieDown:
                UIManager.Instance.lieDownPage.SetActive(true);
                AudioManager.Instance.lieDownAudioSource.Play();
                checkChangeStateCoroutine = StartCoroutine(
                    NotChangeStateUntil(() => SharedInfoManager.Instance.GetIsLying(), State.RecordGripWidth)
                );
                break;

            case State.RecordGripWidth:
                UIManager.Instance.recordGripWidthPage.SetActive(true);
                AudioManager.Instance.recordGripWidthAudioSource.Play();
                checkChangeStateCoroutine = StartCoroutine(
                    NotChangeStateUntil(() => SharedInfoManager.Instance.GetIsOkayRickyDetected(), State.DetectHands)
                );
                break;

            case State.DetectHands:
                //UIManager.Instance.detectHandsPage.SetActive(true);
                AudioManager.Instance.detectHandsAudioSource.Play();
                VisualizationManager.Instance.gripMarkers.SetActive(true);
                checkChangeStateCoroutine = StartCoroutine(
                    NotChangeStateUntil(() => SharedInfoManager.Instance.GetIsHandPositionCorrect(), State.EnterVRMode)
                );
                break;

            case State.EnterVRMode:
                UIManager.Instance.enterVRModePage.SetActive(true);
                AudioManager.Instance.enterVRModeAudioSource.Play();
                checkChangeStateCoroutine = StartCoroutine(
                    NotChangeStateUntil(() => SharedInfoManager.Instance.GetIsOkayRickyDetected(), State.Train)
                );
                break;

            case State.Train:
                //UIManager.Instance.trainPage.SetActive(true);
                VisualizationManager.Instance.barPositionIndicators.SetActive(true);
                VisualizationManager.Instance.barRotationIndicators.SetActive(true);
                TrainCoroutineManager.Instance.StartAllTrainCoroutines();
                break;

            default:
                Debug.LogError($"Unhandled state: {currentState}");
                break;
        }
    }

    private IEnumerator NotChangeStateUntil(Func<bool> condition, State nextState)
    {
        yield return new WaitUntil(condition);
        ChangeState(nextState);
    }
}
