﻿using System.Collections;
using System.Collections.Generic;
using DevionGames.UIWidgets;
using UnityEngine;

namespace DevionGames.InventorySystem
{
    public class ItemTooltip : Tooltip
    {
        public void Show(Item item){
            Show(UnityTools.ColorString(item.Name, item.Rarity.Color),item.Description,item.Icon,item.GetPropertyInfo());
        }
    }
}