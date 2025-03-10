﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames.InventorySystem.ItemActions
{
    [Icon("Item")]
    [ComponentMenu("Inventory System/Add Item")]
    [System.Serializable]
    public class AddItem : ItemAction
    {
        [SerializeField]
        private string m_WindowName = "Inventory";
        [ItemPicker(true)]
        [SerializeField]
        private Item m_Item = null;

        public override ActionStatus OnUpdate()
        {
            Item instance = ScriptableObject.Instantiate(this.m_Item);
            if (this.m_Item.IsCraftable)
            {
                for (int j = 0; j < this.m_Item.ingredients.Count; j++)
                {
                    instance.ingredients[j].item = ScriptableObject.Instantiate(this.m_Item.ingredients[j].item);
                    instance.ingredients[j].item.Stack = this.m_Item.ingredients[j].amount;
                }
            }
            if (ItemContainer.AddItem(this.m_WindowName, instance)) {
                return ActionStatus.Success;
            }
            return ActionStatus.Failure;

        }
    }
}