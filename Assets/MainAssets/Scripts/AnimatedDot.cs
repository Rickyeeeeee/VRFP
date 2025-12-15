using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedDot : MonoBehaviour
{
    [Header("Animation Settings")]
    public Texture2D[] frames;
    public RectTransform rectTransform;
    public RawImage dotObject;
    [SerializeField] private float frameRate = 6f; // Frames per second
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private bool loop = true;
    
    [Header("Frame Sizes")]
    [SerializeField] private Vector2[] frameSizes; // Size for each frame
    [SerializeField] private bool useFrameSizes = true; // Toggle to enable/disable frame sizing
    [SerializeField] private bool debugSizeChanges = false; // Debug size changes
    
    [Header("Animation Direction")]
    [SerializeField] private bool usePingPongAnimation = true; // Use 0->4->0 pattern
    
    private int currentFrameIndex = 0;
    private float frameTimer = 0f;
    private bool isPlaying = false;
    private bool isReversing = false; // Track if we're going backwards in ping-pong mode

    // Start is called before the first frame update
    void Start()
    {
        // Auto-assign rectTransform if not set
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null && dotObject != null)
            {
                rectTransform = dotObject.GetComponent<RectTransform>();
            }
        }
        
        // Auto-assign dotObject if not set
        if (dotObject == null)
        {
            dotObject = GetComponent<RawImage>();
        }
        
        if (debugSizeChanges)
        {
            Debug.Log($"AnimatedDot Start - RectTransform: {rectTransform != null}, DotObject: {dotObject != null}");
            if (rectTransform != null)
            {
                Debug.Log($"Initial size: {rectTransform.sizeDelta}");
            }
        }
        
        if (autoPlay && frames.Length > 0)
        {
            PlayAnimation();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying && frames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            
            // Check if it's time to switch to the next frame
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                NextFrame();
            }
        }
    }
    
    private void NextFrame()
    {
        if (usePingPongAnimation)
        {
            // Ping-pong animation: 0->1->2->3->4->3->2->1->0->1...
            if (!isReversing)
            {
                currentFrameIndex++;
                if (currentFrameIndex >= frames.Length - 1)
                {
                    currentFrameIndex = frames.Length - 1; // Clamp to last frame
                    isReversing = true;
                }
            }
            else
            {
                currentFrameIndex--;
                if (currentFrameIndex <= 0)
                {
                    currentFrameIndex = 0; // Clamp to first frame
                    isReversing = false;
                    
                    // If not looping, stop here
                    if (!loop)
                    {
                        StopAnimation();
                        return;
                    }
                }
            }
        }
        else
        {
            // Standard loop animation: 0->1->2->3->4->0->1...
            currentFrameIndex++;
            
            if (currentFrameIndex >= frames.Length)
            {
                if (loop)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = frames.Length - 1;
                    StopAnimation();
                    return;
                }
            }
        }
        
        // Update the texture and size
        UpdateTexture();
        UpdateSize();
        
        if (debugSizeChanges)
        {
            Debug.Log($"Frame: {currentFrameIndex}, Reversing: {isReversing}, PingPong: {usePingPongAnimation}");
        }
    }
    
    private void UpdateTexture()
    {
        if (dotObject != null && frames.Length > 0 && currentFrameIndex < frames.Length)
        {
            dotObject.texture = frames[currentFrameIndex];
        }
    }
    
    private void UpdateSize()
    {
        if (!useFrameSizes || rectTransform == null || frameSizes.Length == 0 || currentFrameIndex >= frameSizes.Length)
        {
            if (debugSizeChanges && useFrameSizes)
            {
                Debug.Log($"UpdateSize skipped - useFrameSizes: {useFrameSizes}, rectTransform: {rectTransform != null}, frameSizes.Length: {frameSizes.Length}, currentFrameIndex: {currentFrameIndex}");
            }
            return;
        }
        
        Vector2 newSize = frameSizes[currentFrameIndex];
        Vector2 oldSize = rectTransform.sizeDelta;
        
        if (newSize != oldSize)
        {
            rectTransform.sizeDelta = newSize;
            
            if (debugSizeChanges)
            {
                Debug.Log($"Frame {currentFrameIndex}: Size changed from {oldSize} to {newSize}");
            }
        }
    }
    
    public void PlayAnimation()
    {
        if (frames.Length > 0)
        {
            isPlaying = true;
            UpdateTexture(); // Show the current frame immediately
            UpdateSize(); // Update size immediately
        }
    }
    
    public void StopAnimation()
    {
        isPlaying = false;
    }
    
    public void RestartAnimation()
    {
        currentFrameIndex = 0;
        frameTimer = 0f;
        isReversing = false;
        PlayAnimation();
    }
    
    public void SetFrameRate(float newFrameRate)
    {
        frameRate = Mathf.Max(0.1f, newFrameRate); // Minimum frame rate of 0.1 fps
    }
    
    public void SetFrame(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < frames.Length)
        {
            currentFrameIndex = frameIndex;
            UpdateTexture();
            UpdateSize();
        }
    }
    
    public void SetFrameSize(int frameIndex, Vector2 size)
    {
        if (frameIndex >= 0 && frameIndex < frameSizes.Length)
        {
            frameSizes[frameIndex] = size;
            
            // If we're currently on this frame, update the size immediately
            if (currentFrameIndex == frameIndex)
            {
                UpdateSize();
            }
        }
    }
    
    public void SetPingPongMode(bool enabled)
    {
        usePingPongAnimation = enabled;
        if (enabled)
        {
            isReversing = false; // Reset direction when switching to ping-pong
        }
    }
    
    [ContextMenu("Sync Frame Sizes Array")]
    public void SyncFrameSizesArray()
    {
        if (frames.Length > 0)
        {
            System.Array.Resize(ref frameSizes, frames.Length);
            
            // Fill with default size if rectTransform exists
            if (rectTransform != null)
            {
                Vector2 currentSize = rectTransform.sizeDelta;
                for (int i = 0; i < frameSizes.Length; i++)
                {
                    if (frameSizes[i] == Vector2.zero)
                    {
                        frameSizes[i] = currentSize;
                    }
                }
            }
        }
    }
    
    [ContextMenu("Force Update Size")]
    public void ForceUpdateSize()
    {
        UpdateSize();
    }
    
    [ContextMenu("Test Size Change")]
    public void TestSizeChange()
    {
        if (rectTransform != null)
        {
            Vector2 testSize = new Vector2(100f, 100f);
            Debug.Log($"Testing size change to {testSize}");
            rectTransform.sizeDelta = testSize;
        }
    }
}
