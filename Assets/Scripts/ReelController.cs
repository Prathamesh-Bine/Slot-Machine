using System.Collections;
using UnityEngine;
using System;

public class ReelController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform symbolStrip;

    [Header("Spin Settings")]
    public float spinSpeed = 3000f; 
    public float symbolHeight = 115f; 

    [Header("RNG & Buffer Settings")]
    public int totalSymbolsInStrip = 15; // Set this to 15 in the Inspector
    public int numberOfTopBuffers = 2;   // Set this to 2 in the Inspector

    [HideInInspector]
    public int finalSymbolIndex; 

    public IEnumerator SpinReel(float duration, Action onComplete)
    {
        float elapsed = 0f;
        
        // MODIFICATION: Runs "one time less" by subtracting 1.
        float loopThreshold = symbolHeight * (totalSymbolsInStrip - 1);

        // 1. The Visual Blur (Scrolling)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            symbolStrip.anchoredPosition += Vector2.up * spinSpeed * Time.deltaTime;

            if (symbolStrip.anchoredPosition.y >= loopThreshold) 
            {
                symbolStrip.anchoredPosition -= new Vector2(0, loopThreshold);
            }

            yield return null;
        }

        // 2. The True Random Math Engine
        // Pick from the normal symbols (15 total - 2 dummy buffers)
        int mathResult = UnityEngine.Random.Range(0, totalSymbolsInStrip - numberOfTopBuffers);
        
        // 3. The Visual Snap
        int targetCenterIndex = mathResult + numberOfTopBuffers;
        int indexForTopOfWindow = targetCenterIndex - 1; 
        
        // ---> THE MISSING LINE <---
        // You must save the index so GetWinningSymbolName knows where to look!
        finalSymbolIndex = targetCenterIndex; 
        
        float randomSnapY = indexForTopOfWindow * symbolHeight;
        symbolStrip.anchoredPosition = new Vector2(symbolStrip.anchoredPosition.x, randomSnapY);

        onComplete?.Invoke();
    }

    // This looks at the exact center symbol and reads its Sprite name
    public string GetWinningSymbolName()
    {
        // Get the child object sitting at our winning index
        Transform winningChild = symbolStrip.GetChild(finalSymbolIndex);
        
        // Read the name of the picture attached to it
        UnityEngine.UI.Image symbolImage = winningChild.GetComponent<UnityEngine.UI.Image>();
        
        if (symbolImage != null && symbolImage.sprite != null)
        {
            return symbolImage.sprite.name; 
        }
        return "Unknown";
    }
}