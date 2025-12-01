using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetAdjustSettingsPage();
    }

    void Update()
    {
        
    }

    // Canvas
    public GameObject landingPage;
    public GameObject welcomePage;
    public GameObject setupDevicesPage;
    public GameObject adjustSettingsPage;
    public GameObject lieDownPage;
    public GameObject recordGripWidthPage;
    public GameObject detectHandsPage;
    public GameObject enterVRModePage;
    public GameObject trainPage;

    public void HideAllPages()
    {
        //landingPage.SetActive(false);
        welcomePage.SetActive(false);
        setupDevicesPage.SetActive(false);
        adjustSettingsPage.SetActive(false);
        lieDownPage.SetActive(false);
        recordGripWidthPage.SetActive(false);
        //detectHandsPage.SetActive(false);
        enterVRModePage.SetActive(false);
        //trainPage.SetActive(false);
    }

    // Adjust Settings Page
    public TMP_Dropdown numberOfRepetitionsPerSet;
    public TMP_Dropdown numberOfSets;

    public void ResetAdjustSettingsPage()
    {
        //numberOfRepetitionsPerSet.value = 2;
        //numberOfSets.value = 1;
    }

    public void ComfirmAdjustSettingsPage()
    {
        StateManager.Instance.ChangeStateToLieDown();
    }

    // Train Couroutine 1
    public GameObject bar;
    public GameObject redSpot;

    // Train Couroutine 4
    public GameObject chestMuscle;

    // Train Couroutine 5
    public GameObject pressSignalAtLowerPoint;
    public GameObject pressSignalAtLowerPointText;
    public GameObject pressSignalAtHigherPoint;
    public GameObject pressSignalAtHigherPointText;
}
