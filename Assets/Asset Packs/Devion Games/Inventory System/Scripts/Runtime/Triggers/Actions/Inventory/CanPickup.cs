﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.UIWidgets;
using System.Linq;

namespace DevionGames.InventorySystem
{
    [Icon("Condition Item")]
    [ComponentMenu("Inventory System/Can Pickup")]
    public class CanPickup : Action
    {
        [SerializeField]
        private string m_WindowName = "Inventory";

        private ItemCollection m_ItemCollection;

        public override void OnStart()
        {
            this.m_ItemCollection = gameObject.GetComponent<ItemCollection>();
        }

        public override ActionStatus OnUpdate()
        {
            return ItemContainer.CanAddItems(this.m_WindowName,this.m_ItemCollection.ToArray())? ActionStatus.Success: ActionStatus.Failure;
        }
    }

}