using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameControl : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };

    [Header("UI References (3 Required)")]
    [SerializeField]
    private TextMeshProUGUI wonCoinsText;    // 1. Won Coins Text (Top display frame)

    [SerializeField]
    private TextMeshProUGUI totalCoinsText;  // 2. Total Coins Text

    [SerializeField]
    private Button spinButton;               // 3. Spin 50 UI Button

    [Header("Audio Configurations")]
    [SerializeField]
    private AudioSource reelSpinAudioSource; // Audio Source set to Loop

    [SerializeField]
    private AudioSource winAudioSource;      // Audio Source for payouts

    [SerializeField]
    private AudioClip spinSound;             // Continuous reel spinning sound

    [SerializeField]
    private AudioClip winSound;              // Winning payout sound

    [Header("Reel Configuration")]
    [SerializeField]
    private Row[] rows;                     // Drag Row, Row (1), Row (2)

    [SerializeField]
    private Transform handle;                   // Handle Transform for rotation animation

    [Header("Economy Settings")]
    [SerializeField]
    private int totalCoins = 1000;
    private readonly int spinCost = 50;
    private int wonCoins = 0;
    private bool resultsChecked = false;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        // 1. WHILE REELS ARE SPINNING: Loop continuous sound
        if (!rows[0].rowStopped || !rows[1].rowStopped || !rows[2].rowStopped)
        {
            wonCoins = 0;
            resultsChecked = false;

            if (reelSpinAudioSource != null && spinSound != null && !reelSpinAudioSource.isPlaying)
            {
                reelSpinAudioSource.clip = spinSound;
                reelSpinAudioSource.loop = true;
                reelSpinAudioSource.Play();
            }
        }

        // 2. WHEN ALL 3 REELS HAVE STOPPED: Stop loop sound and calculate win
        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped && !resultsChecked)
        {
            if (reelSpinAudioSource != null && reelSpinAudioSource.isPlaying)
            {
                reelSpinAudioSource.Stop();
            }

            CheckResults();
            UpdateUI();
        }
    }

    // Triggered by UI Button OR Handle Click
    public void TriggerSpin()
    {
        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped)
        {
            if (totalCoins < spinCost)
            {
                Debug.Log("Not enough coins to spin!");
                return;
            }

            totalCoins -= spinCost;
            wonCoins = 0;
            UpdateUI();

            StartCoroutine("PullHandle");
        }
    }

    private void OnMouseDown()
    {
        TriggerSpin();
    }

    private IEnumerator PullHandle()
    {
        if (spinButton != null) spinButton.interactable = false;

        // Rotate handle down
        for (int i = 0; i < 15; i += 5)
        {
            handle.Rotate(0f, 0f, i);
            yield return new WaitForSeconds(0.1f);
        }

        // Trigger reel start event
        HandlePulled();

        // Rotate handle back up
        for (int i = 0; i < 15; i += 5)
        {
            handle.Rotate(0f, 0f, -i);
            yield return new WaitForSeconds(0.1f);
        }

        if (spinButton != null) spinButton.interactable = true;
    }

    private void CheckResults()
    {
        string r1 = rows[0].stoppedSlot;
        string r2 = rows[1].stoppedSlot;
        string r3 = rows[2].stoppedSlot;

        // 3 Matching = 300
        if (r1 == r2 && r2 == r3)
        {
            wonCoins = 300;
        }
        // 2 Matching = 200
        else if (r1 == r2 || r2 == r3 || r1 == r3)
        {
            wonCoins = 200;
        }
        // No match = 0
        else
        {
            wonCoins = 0;
        }

        totalCoins += wonCoins;

        // Play win payout sound
        if (wonCoins > 0 && winAudioSource != null && winSound != null)
        {
            winAudioSource.PlayOneShot(winSound);
        }

        resultsChecked = true;
    }

    private void UpdateUI()
    {
        if (wonCoinsText != null) wonCoinsText.text = "WON: " + wonCoins;
        if (totalCoinsText != null) totalCoinsText.text = "COINS: " + totalCoins;
    }
}