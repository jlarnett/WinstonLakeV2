using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Winston.Core;

public class HudDisplay : MonoBehaviour
{
    [SerializeField] private Image healthBarImage = null;
    [SerializeField] private Image EnergyBarImage = null;


    [SerializeField] private TextMeshProUGUI hourText = null;
    [SerializeField] private TextMeshProUGUI minuteText = null;

    private Health health = null;
    private Energy energy;
    private GameTimer timer;

    private void HandleHealthUpdated(int currentHealth)
    {
        //Called when health is updated.
        healthBarImage.fillAmount = (float)currentHealth / health.GetMaxHealth();
    }

    private void HandleEnergyUpdated(int currentEnergy)
    {
        //Called when health is updated.
        EnergyBarImage.fillAmount = (float)currentEnergy / energy.GetMaxEnergy();
    }

    private void Awake()
    {
        health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        energy = GameObject.FindGameObjectWithTag("Player").GetComponent<Energy>();

    }

    private void OnEnable()
    {
        health.onHealthUpdated += HandleHealthUpdated;
        energy.onEnergyUpdated += HandleEnergyUpdated;
        GameTimer.timerUpdated += HandleTimerUpdated;
    }

    private void OnDisable()
    {
        health.onHealthUpdated -= HandleHealthUpdated;
        energy.onEnergyUpdated -= HandleEnergyUpdated;
        GameTimer.timerUpdated -= HandleTimerUpdated;
    }

    private void HandleTimerUpdated(int timerFloat)
    {
        int hourResult = Convert.ToInt32(Math.Floor(timerFloat/60f));
        int minuteResult = timerFloat % 60;

        hourText.text = hourResult.ToString();

        minuteText.text = minuteResult.ToString();

        if (minuteText.text.Length == 1)
        {
            minuteText.text = "0" + minuteText.text;
        }
    }
}
