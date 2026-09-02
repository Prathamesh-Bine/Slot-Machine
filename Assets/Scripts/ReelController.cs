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
    public int totalSymbolsInStrip = 15; 
    public int numberOfTopBuffers = 2;   

    [HideInInspector]
    public int finalSymbolIndex; 

    public IEnumerator SpinReel(float duration, Action onComplete)
    {
        float elapsed = 0f;
        
        // Calculates the exact point to teleport back for an infinite loop
        float loopThreshold = symbolHeight * (totalSymbolsInStrip - 1);

        // Phase 1: Scrolling Animation
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

        // Phase 2: RNG & Visual Snap
        // Picks a random index, ignoring the fake buffer symbols at the top
        int mathResult = UnityEngine.Random.Range(0, totalSymbolsInStrip - numberOfTopBuffers);
        
        int targetCenterIndex = mathResult + numberOfTopBuffers;
        int indexForTopOfWindow = targetCenterIndex - 1; 
        
        finalSymbolIndex = targetCenterIndex; 
        
        // Snaps the target symbol perfectly into the window
        float randomSnapY = indexForTopOfWindow * symbolHeight;
        symbolStrip.anchoredPosition = new Vector2(symbolStrip.anchoredPosition.x, randomSnapY);

        onComplete?.Invoke();
    }

    public string GetWinningSymbolName()
    {
        Transform winningChild = symbolStrip.GetChild(finalSymbolIndex);
        UnityEngine.UI.Image symbolImage = winningChild.GetComponent<UnityEngine.UI.Image>();
        
        if (symbolImage != null && symbolImage.sprite != null)
        {
            return symbolImage.sprite.name; 
        }
        
        return "Unknown"; 
    }
}