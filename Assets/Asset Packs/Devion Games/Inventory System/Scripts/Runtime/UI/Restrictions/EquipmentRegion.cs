using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames.InventorySystem.Restrictions
{
    public class EquipmentRegion : Restriction
    {
        [EquipmentPicker(true)]
        public DevionGames.InventorySystem.EquipmentRegion region;

        public override bool CanAddItem(Item item)
        {
            if (item.GetType() != typeof(EquipmentItem))
            {
                return false;
            }
            EquipmentItem mItem = item as EquipmentItem;
            for (int i = 0; i < mItem.Region.Count; i++)
            {
                if (mItem.Region[i].Name == region.Name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}