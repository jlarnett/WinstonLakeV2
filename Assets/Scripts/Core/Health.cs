using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    [SerializeField] private int maxhealth = 100;
    [SerializeField] private int currentHealth;

    private bool isDead = false;

    public event Action onDie;
    public event Action<int> onHealthUpdated;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxhealth;
    }

    public int GetMaxHealth()
    {
        return maxhealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        onHealthUpdated?.Invoke(currentHealth);

        if (currentHealth.Equals(0))
        {
            //Die
            onDie?.Invoke();
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
    }
}
