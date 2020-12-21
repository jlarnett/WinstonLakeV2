using DevionGames.InventorySystem;
using UnityEngine;

namespace Winston.Inventory
{
    public class Tool : Weapon
    {
        [SerializeField] private ToolType toolType;
        [SerializeField] private int energyConsumption = 5;

        public int GetEnergyConsumption()
        {
            return energyConsumption;
        }
    }
}
