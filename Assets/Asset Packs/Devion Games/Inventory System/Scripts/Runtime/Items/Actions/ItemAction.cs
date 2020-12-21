using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames.InventorySystem.ItemActions
{
    [System.Serializable]
    public abstract class ItemAction : Action
    {
        [HideInInspector]
        public Item item;

    }
}