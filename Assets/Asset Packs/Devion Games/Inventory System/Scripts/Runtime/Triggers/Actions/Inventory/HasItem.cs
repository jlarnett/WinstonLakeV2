﻿using System.Collections;
using System.Collections.Generic;
using DevionGames.UIWidgets;
using UnityEngine;

namespace DevionGames.InventorySystem
{
    [Icon("Condition Item")]
    [ComponentMenu("Inventory System/Has Item")]
    public class HasItem : Action
    {
        [SerializeField]
        protected ItemCondition[] requiredItems;

        public override ActionStatus OnUpdate()
        {
            for (int i = 0; i < requiredItems.Length; i++)
            {
                ItemCondition condition = requiredItems[i];
                if (condition.item != null && !string.IsNullOrEmpty(condition.stringValue)) { 

                    if (!ItemContainer.HasItem(condition.stringValue,condition.item, 1))
                    {
                        if (InventoryManager.UI.notification != null)
                        {
                            InventoryManager.UI.notification.AddItem(InventoryManager.Notifications.missingItem,condition.item.Name,condition.stringValue);
                        }
                        return ActionStatus.Failure;
                    }
                }
            }

            return ActionStatus.Success;
        }
    }
}