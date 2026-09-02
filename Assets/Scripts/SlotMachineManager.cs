using System.Collections;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

public class SlotMachineManager : MonoBehaviour
{
    [Header("Machine Setup")]
    public ReelController[] reels; 

    [Header("UI References")]
    public TextMeshProUGUI balanceText; 
    public TextMeshProUGUI currentBetText; 
    
    [Header("Betting Controls")]
    public Button[] betButtons; 
    public int currentBet = 10; 

    [Header("Lever Animation")]
    public GameObject leverUpObject; 
    public GameObject leverDownObject; 

    [Header("Popup References")]
    public GameObject jackpotPopup; 
    public TextMeshProUGUI popupText; 
    
    [Header("Visual Effects")]
    public ParticleSystem moneyRainParticles; 

    [Header("Audio Settings")]
    public AudioSource spinAudioSource; 
    public AudioSource uiAudioSource; 
    public AudioClip clickSound;      
    public AudioClip cashoutSound;    
    public AudioClip jackpotSound; 

    [Header("Game State")]
    // Starts the player with 50G by default
    public int balance = 2000; 
    
    private bool isSpinning = false;
    private int reelsStopped = 0;

    void Start()
    {
        UpdateUI();
        
        // Ensures the jackpot popup is hidden when the game first loads
        if (jackpotPopup != null) 
        {
            jackpotPopup.SetActive(false);
        }
        
        // Resets the lever visual to the default 'up' position
        if (leverUpObject != null && leverDownObject != null)
        {
            leverUpObject.SetActive(true);
            leverDownObject.SetActive(false);
        }
    }

    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    public void SetBet(int amount)
    {
        // Prevents changing the bet while the machine is currently rolling
        if (isSpinning) return;
        
        PlayClickSound(); 
        
        currentBet = amount;
        Debug.Log($"Bet changed to: {currentBet}G");
        
        UpdateUI();
    }

    public void SetBetAndSpin(int amount)
    {
        SetBet(amount);
        Spin();
    }

    public void Spin()
    {
        // Prevents double-clicking the spin button
        if (isSpinning) return;

        // Checks if the player has enough money to cover their current bet size
        if (balance < currentBet)
        {
            Debug.Log("Not enough balance!");
            PlayClickSound(); 
            StartCoroutine(ShowWarningRoutine("INSUFFICIENT\nBALANCE!"));
            return;
        }

        PlayClickSound(); 

        // Deducts the bet amount upfront before the spin starts
        balance -= currentBet;
        UpdateUI();
        
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        reelsStopped = 0;

        if (spinAudioSource != null) spinAudioSource.Play();

        // Locks all UI buttons so the player cannot interfere during the animation
        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = false;
        }

        // Fakes the lever pull animation: swaps sprites, waits a fraction of a second, then swaps back
        if (leverUpObject != null && leverDownObject != null)
        {
            leverUpObject.SetActive(false);
            leverDownObject.SetActive(true);
            
            yield return new WaitForSeconds(0.2f);
            
            leverDownObject.SetActive(false);
            leverUpObject.SetActive(true);
        }

        // Triggers each reel to spin. Adds a slight delay (i * 0.5f) so they stop one after the other.
        for (int i = 0; i < reels.Length; i++)
        {
            float spinDuration = 2.0f + (i * 0.5f);
            StartCoroutine(reels[i].SpinReel(spinDuration, OnReelStopped));
        }
    }

    private void OnReelStopped()
    {
        reelsStopped++;

        // Only evaluates the win condition after the final reel has finished its animation
        if (reelsStopped >= reels.Length)
        {
            EvaluateWin();
        }
    }

    private void EvaluateWin()
    {
        // Grabs the final resting symbols from all three reels
        string reel1Symbol = reels[0].GetWinningSymbolName();
        string reel2Symbol = reels[1].GetWinningSymbolName();
        string reel3Symbol = reels[2].GetWinningSymbolName();

        Debug.Log($"Results: {reel1Symbol} | {reel2Symbol} | {reel3Symbol}");

        // Logic check: Checks if all three symbols match
        if (reel1Symbol == reel2Symbol && reel2Symbol == reel3Symbol)
        {
            int multiplier = 0;

            // Assigns payout multipliers based on which symbol was matched
            switch (reel1Symbol)
            {
                case "slot-symbol1_0": 
                    multiplier = 50; 
                    break;
                case "slot-symbol2_0": 
                    multiplier = 10;
                    break;
                case "slot-symbol3_0": 
                    multiplier = 25;
                    break;
                case "slot-symbol4":   
                    multiplier = 100;
                    break;
                default:
                    multiplier = 5; // Fallback payout
                    break;
            }

            // Calculates total payout by multiplying their bet by the symbol's value
            int winAmount = currentBet * multiplier; 
            balance += winAmount;
            
            Debug.Log($"JACKPOT! Matched {reel1Symbol} for {winAmount}G!");
            StartCoroutine(ShowJackpotRoutine(winAmount));
        }
        else
        {
            Debug.Log("No Win.");
        }

        UpdateUI();
        isSpinning = false;
        
        if (spinAudioSource != null) spinAudioSource.Stop();
        
        // Unlocks the UI buttons for the next round
        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = true;
        }
    }

    // Uses a separate routine just for warnings so it doesn't trigger the money rain
    private IEnumerator ShowWarningRoutine(string message)
    {
        if (jackpotPopup != null)
        {
            if (popupText != null)
            {
                popupText.text = message;
            }

            jackpotPopup.SetActive(true);
            yield return new WaitForSeconds(2.0f); 
            jackpotPopup.SetActive(false);
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

            // Fires off the visual and audio celebration effects
            if (moneyRainParticles != null) moneyRainParticles.Play(); 
            
            if (uiAudioSource != null && jackpotSound != null)
            {
                uiAudioSource.PlayOneShot(jackpotSound);
            }

            // Leaves the popup on screen long enough for the player to enjoy the win
            jackpotPopup.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            jackpotPopup.SetActive(false);
            
            if (moneyRainParticles != null) moneyRainParticles.Stop();
        }
    }

    public void Checkout()
    {
        if (isSpinning) return; 
        StartCoroutine(CheckoutRoutine());
    }

    private IEnumerator CheckoutRoutine()
    {
        if (uiAudioSource != null && cashoutSound != null)
        {
            uiAudioSource.PlayOneShot(cashoutSound);
        }

        Debug.Log($"Player cashed out with {balance}G!");

        // Locks buttons during the cashout animation so they can't spin while it resets
        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = false;
        }

        if (jackpotPopup != null && popupText != null)
        {
            popupText.text = $"CASHED OUT:\n{balance}G!";
            jackpotPopup.SetActive(true);
            
            yield return new WaitForSeconds(3.0f); 
            
            jackpotPopup.SetActive(false);
        }

        // Resets the game back to the starting balance for a fresh session
        balance = 2000;
        UpdateUI();

        // Unlocks buttons for the new game
        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = true;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        // Note: This only works in a built executable or WebGL, not in the Unity Editor
        Application.Quit();
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