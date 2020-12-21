using UnityEngine;
using Winston.Inventory;

public class WaterCan : Tool
{
    [SerializeField] private int MaxUses = 30;
    private int remainingUses = 0;

    private Energy energy = null;

    protected override void Awake()
    {
        base.Awake();
        energy = GameObject.FindGameObjectWithTag("Player").GetComponent<Energy>();
    }

    protected override bool CanUnuse()
    {
        return base.CanUnuse();
    }

    protected override bool CanUse()
    {
        return base.CanUse();
    }

    protected override void OnStartUse()
    {
        base.OnStartUse();
    }

    protected override void OnStopUse()
    {
        base.OnStopUse();
    }


    public void UseCan()
    {
        if (remainingUses <= 0) return;
        remainingUses = Mathf.Max(0, remainingUses - 1);

        if (energy == null) return;
        energy.DrainEnergy(this.GetEnergyConsumption());
    }
}
