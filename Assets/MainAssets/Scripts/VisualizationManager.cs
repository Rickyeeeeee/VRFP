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
        gripMarkers.transform.position = bar.transform.position;
        gripMarkers.transform.rotation = bar.transform.rotation;
    }

    // Visualization
    public GameObject bar;
    public GameObject bench;
    public GameObject gripMarkers;
    public GameObject barPositionIndicators;
    public GameObject barRotationIndicators;

    public void HideAllVisualization()
    {
        bar.SetActive(false);
        bench.SetActive(false);
        gripMarkers.SetActive(false);
        barPositionIndicators.SetActive(false);
        barRotationIndicators.SetActive(false);
        smpl.SetActive(false);
    }

    // Train Couroutine 2
    public GameObject upperIndicator;
    public GameObject lowerIndicator;

    // Train Couroutine 3
    public GameObject leftIndicator;
    public GameObject rightIndicator;

    public GameObject smpl;
}
