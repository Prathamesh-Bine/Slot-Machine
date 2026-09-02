using System.Collections;
using UnityEngine;
using TMPro; // Required for TextMeshPro UI
using UnityEngine.UI; // Required for UI components like Buttons

public class SlotMachineManager : MonoBehaviour
{
    [Header("Machine Setup")]
    public ReelController[] reels; // Array to hold your 3 reels

    [Header("UI References")]
    public TextMeshProUGUI balanceText; // The text showing your balance (e.g., "50G")
    public TextMeshProUGUI currentBetText; // The text showing your currently selected bet
    
    [Header("Betting Controls")]
    public Button[] betButtons; // Array to hold your different bet buttons (10, 50, Max Bet)
    public int currentBet = 10; // Default starting bet

    [Header("Lever Animation")]
    public GameObject leverUpObject; // The default lever pointing up
    public GameObject leverDownObject; // The lever pulled down

    [Header("Popup References")]
    public GameObject jackpotPopup; // Drag your 'Jackpot_popup_Window' parent here
    public TextMeshProUGUI popupText; // Drag the child text object here

    [Header("Game State")]
    public int balance = 50;
    
    private bool isSpinning = false;
    private int reelsStopped = 0;

    void Start()
    {
        // Ensure initial UI matches starting balance, current bet text, and states
        UpdateUI();
        
        if (jackpotPopup != null) 
        {
            jackpotPopup.SetActive(false);
        }
        
        if (leverUpObject != null && leverDownObject != null)
        {
            leverUpObject.SetActive(true);
            leverDownObject.SetActive(false);
        }
    }

    // Hook this up to your individual bet buttons via OnClick in the Inspector
    public void SetBet(int amount)
    {
        if (isSpinning) return;
        currentBet = amount;
        Debug.Log($"Bet changed to: {currentBet}G");
        
        // Refresh UI immediately to show the updated bet text
        UpdateUI();
    }

    // Helper method to set the bet and immediately trigger a spin (great for Max Bet)
    public void SetBetAndSpin(int amount)
    {
        SetBet(amount);
        Spin();
    }

    // Hook this up to your main Spin / Lever button's OnClick event
    public void Spin()
    {
        if (isSpinning || balance < currentBet)
        {
            Debug.Log("Not enough balance or already spinning!");
            return;
        }

        // Deduct current custom bet and update text
        balance -= currentBet;
        UpdateUI();
        
        // Start the game loop
        StartCoroutine(SpinRoutine());
    }
    // Hook this up to your new Checkout button
    public void Checkout()
    {
        // Don't let them cash out while the reels are actively spinning
        if (isSpinning) return; 

        Debug.Log($"Player cashed out with {balance}G!");

        // Reuse your existing popup window to show the final cash-out amount
        if (jackpotPopup != null && popupText != null)
        {
            popupText.text = $"CASHED OUT:\n{balance}G!";
            jackpotPopup.SetActive(true);
        }

        // Zero out the balance and update the text to show the game is over
        balance = 0;
        UpdateUI();

        // Lock all betting buttons so they can't play anymore
        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = false;
        }
    }


    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        reelsStopped = 0;

        // 1. Lock all betting buttons so the player can't change bets or spam mid-spin
        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = false;
        }

        // 2. Animate Lever using GameObject toggles
        if (leverUpObject != null && leverDownObject != null)
        {
            leverUpObject.SetActive(false);
            leverDownObject.SetActive(true);
            
            yield return new WaitForSeconds(0.2f);
            
            leverDownObject.SetActive(false);
            leverUpObject.SetActive(true);
        }

        // 3. Trigger Reels with staggered stop times
        for (int i = 0; i < reels.Length; i++)
        {
            // Reel 1 spins for 2s, Reel 2 for 2.5s, Reel 3 for 3s
            float spinDuration = 2.0f + (i * 0.5f);
            
            // Start the coroutine on the ReelController and pass the callback method
            StartCoroutine(reels[i].SpinReel(spinDuration, OnReelStopped));
        }
    }

    // This is called automatically by each reel when it finishes snapping into place
    private void OnReelStopped()
    {
        reelsStopped++;

        // Once all 3 reels report they have stopped, evaluate the board
        if (reelsStopped >= reels.Length)
        {
            EvaluateWin();
        }
    }

    private void EvaluateWin()
    {
        // 1. Ask each reel what symbol landed in the center
        string reel1Symbol = reels[0].GetWinningSymbolName();
        string reel2Symbol = reels[1].GetWinningSymbolName();
        string reel3Symbol = reels[2].GetWinningSymbolName();

        Debug.Log($"Results: {reel1Symbol} | {reel2Symbol} | {reel3Symbol}");

        // 2. Check if all three symbols are exactly the same
        if (reel1Symbol == reel2Symbol && reel2Symbol == reel3Symbol)
        {
            int multiplier = 0;

            // 3. The Paytable: Assign multipliers based on the exact sprite name
            switch (reel1Symbol)
            {
                case "slot-symbol1_0": // e.g., The 7s
                    multiplier = 50; 
                    break;
                case "slot-symbol2_0": // e.g., The Cherries
                    multiplier = 10;
                    break;
                case "slot-symbol3_0": // e.g., The Bells
                    multiplier = 25;
                    break;
                case "slot-symbol4":   // e.g., The BARs
                    multiplier = 100;
                    break;
                default:
                    // Fallback multiplier just in case
                    multiplier = 5;
                    break;
            }

            // Calculate the final win using the customized dynamic bet amount
            int winAmount = currentBet * multiplier; 
            balance += winAmount;
            
            Debug.Log($"JACKPOT! Matched {reel1Symbol} for {winAmount}G!");

            // Trigger the celebration popup window with the calculated win amount
            StartCoroutine(ShowJackpotRoutine(winAmount));
        }
        else
        {
            Debug.Log("No Win.");
        }

        // 4. Update the text and unlock all betting buttons for the next round
        UpdateUI();
        isSpinning = false;
        
        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = true;
        }
    }

    private IEnumerator ShowJackpotRoutine(int amount)
    {
        if (jackpotPopup != null)
        {
            if (popupText != null)
            {
                popupText.text = $"YOU WON {amount}G!";
            }

            // Enable parent window to show both image and text simultaneously
            jackpotPopup.SetActive(true);
            
            // Wait for 2 seconds while the player celebrates
            yield return new WaitForSeconds(2.0f);
            
            // Hide the window again
            jackpotPopup.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (balanceText != null)
        {
            balanceText.text = balance.ToString() + "G"; 
        }

        if (currentBetText != null)
        {
            currentBetText.text = "Bet: " + currentBet.ToString() + "G";
        }
    }
}