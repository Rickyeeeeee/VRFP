using UnityEngine;
using System.Collections;

public class VisualizationManager : MonoBehaviour
{
    public static VisualizationManager Instance { get; private set; }

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

    // Visualization
    public GameObject gripMarkers;
    public GameObject barPositionIndicators;
    public GameObject barRotationIndicators;

    public void HideAllVisualization()
    {
        gripMarkers.SetActive(false);
        barPositionIndicators.SetActive(false);
        barRotationIndicators.SetActive(false);
    }

    // Train Couroutine 2
    public GameObject upperIndicator;
    public GameObject lowerIndicator;

    // Train Couroutine 3
    public GameObject leftIndicator;
    public GameObject rightIndicator;
}
