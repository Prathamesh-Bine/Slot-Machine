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

    [Header("Audio Settings")]
    public AudioSource spinAudioSource; 
    public AudioSource uiAudioSource; 
    public AudioClip clickSound;      
    public AudioClip cashoutSound;    // <-- NEW: Link your unique cashout sound here

    [Header("Game State")]
    public int balance = 50;
    
    private bool isSpinning = false;
    private int reelsStopped = 0;

    void Start()
    {
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

    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    public void SetBet(int amount)
    {
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
        if (isSpinning || balance < currentBet)
        {
            Debug.Log("Not enough balance or already spinning!");
            return;
        }

        PlayClickSound(); 

        balance -= currentBet;
        UpdateUI();
        
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        reelsStopped = 0;

        if (spinAudioSource != null) spinAudioSource.Play();

        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = false;
        }

        if (leverUpObject != null && leverDownObject != null)
        {
            leverUpObject.SetActive(false);
            leverDownObject.SetActive(true);
            
            yield return new WaitForSeconds(0.2f);
            
            leverDownObject.SetActive(false);
            leverUpObject.SetActive(true);
        }

        for (int i = 0; i < reels.Length; i++)
        {
            float spinDuration = 2.0f + (i * 0.5f);
            StartCoroutine(reels[i].SpinReel(spinDuration, OnReelStopped));
        }
    }

    private void OnReelStopped()
    {
        reelsStopped++;

        if (reelsStopped >= reels.Length)
        {
            EvaluateWin();
        }
    }

    private void EvaluateWin()
    {
        string reel1Symbol = reels[0].GetWinningSymbolName();
        string reel2Symbol = reels[1].GetWinningSymbolName();
        string reel3Symbol = reels[2].GetWinningSymbolName();

        Debug.Log($"Results: {reel1Symbol} | {reel2Symbol} | {reel3Symbol}");

        if (reel1Symbol == reel2Symbol && reel2Symbol == reel3Symbol)
        {
            int multiplier = 0;

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
                    multiplier = 5;
                    break;
            }

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

            jackpotPopup.SetActive(true);
            yield return new WaitForSeconds(2.0f);
            jackpotPopup.SetActive(false);
        }
    }

    public void Checkout()
    {
        if (isSpinning) return; 

        // <-- NEW: Play the unique cashout sound instead of the regular click
        if (uiAudioSource != null && cashoutSound != null)
        {
            uiAudioSource.PlayOneShot(cashoutSound);
        }

        Debug.Log($"Player cashed out with {balance}G!");

        if (jackpotPopup != null && popupText != null)
        {
            popupText.text = $"CASHED OUT:\n{balance}G!";
            jackpotPopup.SetActive(true);
        }

        balance = 0;
        UpdateUI();

        foreach (Button btn in betButtons)
        {
            if (btn != null) btn.interactable = false;
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