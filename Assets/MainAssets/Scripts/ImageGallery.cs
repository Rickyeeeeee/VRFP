using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageGallery : MonoBehaviour
{
    [Header("Animation Settings")]
    private RawImage rawImage;
    [SerializeField] private float transitionDuration = 0.5f; // Animation time
    [SerializeField] private float holdDuration = 2.0f; // Time to hold each position
    
    [Header("Animation Style")]
    [SerializeField] private bool useInstantReset = true; // Flag to choose animation style
    [Tooltip("True: 0->0.5 animated, 0.5->0 instant\nFalse: Both directions animated")]
    
    [Header("Dot Indicators")]
    public Image dot1;
    public Image dot2;
    [SerializeField] private Color enableColor = new Color(0.207f, 0.961f, 0.835f, 1f); // #35F5D5
    [SerializeField] private Color disableColor = new Color(1f, 1f, 1f, 1f); // #FFFFFF
    
    private float startX = 0.0f;
    private float targetX = 0.5f;
    private bool isAnimating = false;
    private bool isHolding = false;
    private float timer = 0.0f;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        // Start with holding the initial position
        UpdateDotColors();
        StartHolding();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (isHolding)
        {
            // Hold current position for specified duration
            if (timer >= holdDuration)
            {
                StartTransition();
            }
        }
        else if (isAnimating)
        {
            // Animate to target position
            float progress = timer / transitionDuration;
            
            // Use Lerp for smooth animation
            float currentX = Mathf.Lerp(startX, targetX, progress);
            
            // Create a new Rect with animated x coordinate
            Rect uvRect = rawImage.uvRect;
            uvRect.x = currentX;
            rawImage.uvRect = uvRect;
            
            // When transition completes, start holding at new position
            if (progress >= 1.0f)
            {
                // Ensure exact final value
                uvRect.x = targetX;
                rawImage.uvRect = uvRect;
                
                // Update dot colors when position changes
                UpdateDotColors();
                
                // For ping-pong style, swap start and target
                if (!useInstantReset)
                {
                    float temp = startX;
                    startX = targetX;
                    targetX = temp;
                }
                
                StartHolding();
            }
        }
    }
    
    private void UpdateDotColors()
    {
        if (dot1 == null || dot2 == null) return;
        
        // Get current UV position
        float currentX = rawImage.uvRect.x;
        
        if (Mathf.Approximately(currentX, 0.0f))
        {
            // At position 0 - first image active
            dot1.color = enableColor;
            dot2.color = disableColor;
        }
        else if (Mathf.Approximately(currentX, 0.5f))
        {
            // At position 0.5 - second image active
            dot1.color = disableColor;
            dot2.color = enableColor;
        }
    }
    
    private void StartHolding()
    {
        isAnimating = false;
        isHolding = true;
        timer = 0.0f;
    }
    
    private void StartTransition()
    {
        if (useInstantReset)
        {
            // Instant reset style: check current position
            Rect currentRect = rawImage.uvRect;
            
            if (currentRect.x == 0.0f)
            {
                // Animate from 0 to 0.5
                startX = 0.0f;
                targetX = 0.5f;
                isHolding = false;
                isAnimating = true;
                timer = 0.0f;
            }
            else if (currentRect.x == 0.5f)
            {
                // Instantly jump from 0.5 to 0 (no animation)
                Rect uvRect = rawImage.uvRect;
                uvRect.x = 0.0f;
                rawImage.uvRect = uvRect;
                UpdateDotColors(); // Update dots immediately for instant reset
                StartHolding();
            }
        }
        else
        {
            // Ping-pong style: always animate
            isHolding = false;
            isAnimating = true;
            timer = 0.0f;
        }
    }
    
    public void StopAnimation()
    {
        isAnimating = false;
        isHolding = false;
    }
    
    public void ToggleAnimationStyle()
    {
        useInstantReset = !useInstantReset;
        Debug.Log($"Animation style changed to: {(useInstantReset ? "Instant Reset" : "Ping-Pong")}");
    }
    
    public void SetInstantResetStyle(bool instant)
    {
        useInstantReset = instant;
    }
}
