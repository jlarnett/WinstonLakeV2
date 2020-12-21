using System.Collections;
using System.Collections.Generic;
using DevionGames.UIWidgets;
using UnityEngine;

namespace DevionGames.InventorySystem
{
    [Icon("Item")]
    [ComponentMenu("Inventory System/Save")]
    public class Save : Action
    {
        public override ActionStatus OnUpdate()
        {
            InventoryManager.Save(); 
            return ActionStatus.Success;
        }
    }
}
