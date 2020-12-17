using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int currentEnergy;

    private bool isExhausted = false;

    public event Action onExhausted;
    public event Action<int> onEnergyUpdated;

    // Start is called before the first frame update
    void Start()
    {
        currentEnergy = maxEnergy;
    }

    public int GetMaxEnergy()
    {
        return maxEnergy;
    }

    public void DrainEnergy(int damage)
    {
        currentEnergy = Mathf.Max(currentEnergy - damage, 0);
        onEnergyUpdated?.Invoke(currentEnergy);

        if (currentEnergy.Equals(0))
        {
            //Die
            onExhausted?.Invoke();
            Exhausted();
        }
    }

    private void Exhausted()
    {
        if (isExhausted) return;
        isExhausted = true;
    }
}
