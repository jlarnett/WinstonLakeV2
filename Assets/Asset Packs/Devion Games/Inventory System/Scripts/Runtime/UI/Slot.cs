﻿using System.Collections;
using System.Collections.Generic;
using DevionGames.UIWidgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DevionGames.InventorySystem
{
    public class Slot : CallbackHandler
    {
        /// <summary>
        /// The text to display item name.
        /// </summary>
        [SerializeField]
        protected Text m_ItemName;
        /// <summary>
        /// The Image to display item icon.
        /// </summary>
        [SerializeField]
        protected Image m_Ícon;
        /// <summary>
		/// The text to display item stack.
		/// </summary>
		[SerializeField]
        protected Text m_Stack;

        //Actions to run when the trigger is used.
        [HideInInspector]
        public List<Restriction> restrictions = new List<Restriction>();

        private Item m_Item;
        /// <summary>
        /// The item this slot is holding
        /// </summary>
        public Item ObservedItem{
            get {
                return this.m_Item;
            }
            set {
                this.m_Item = value;

                if (this.m_Item != null) {
                    if (!Container.UseReferences){
                        this.m_Item.Slot = this;
                    }else{
                        if (!this.m_Item.ReferencedSlots.Contains(this))
                        {
                            this.m_Item.ReferencedSlots.Add(this);
                        }
                    }
                }
                Repaint();
            }
        }

        /// <summary>
        /// Checks if the slot is empty ObservedItem == null
        /// </summary>
        public bool IsEmpty {
            get { return ObservedItem == null; }
        }

        private ItemContainer m_Container;
        /// <summary>
        /// The item container that holds this slot
        /// </summary>
        public ItemContainer Container {
            get { return this.m_Container;}
            set { this.m_Container = value; }
        }

        private int m_Index = -1;
        /// <summary>
        /// Index of item container
        /// </summary>
        public int Index {
            get { return this.m_Index; }
            set { this.m_Index = value; }
        }

        public override string[] Callbacks {
            get
            {
                List<string> callbacks = new List<string>();
                callbacks.Add("OnAddItem");
                callbacks.Add("OnRemoveItem");
                callbacks.Add("OnUseItem");
                return callbacks.ToArray();
            }
        }

        protected virtual void Start() {

            Container.OnAddItem += (Item item, Slot slot) => {
                if (slot == this){
                    ItemEventData eventData = new ItemEventData(item);
                    eventData.slot = slot;
                    Execute("OnAddItem", eventData);
                }

            };
            Container.OnRemoveItem += (Item item, int amount, Slot slot) => {
                if (slot == this){
                    ItemEventData eventData = new ItemEventData(item);
                    eventData.slot = slot;
                    Execute("OnRemoveItem", eventData);
                }

            };
            Container.OnUseItem += (Item item, Slot slot) => {
                if (slot == this)
                {
                    ItemEventData eventData = new ItemEventData(item);
                    eventData.slot = slot;
                    Execute("OnUseItem", eventData);
                }
            };
            
        }

        /// <summary>
        /// Repaint slot visuals with item information
        /// </summary>
        public virtual void Repaint()
        {
            if (this.m_ItemName != null){
                //Updates the text with item name and rarity color. If this slot is empty, sets the text to empty.
                this.m_ItemName.text = (!IsEmpty ? UnityTools.ColorString(ObservedItem.Name, ObservedItem.Rarity.Color) : string.Empty);
            }

            if (this.m_Ícon != null){
                if (!IsEmpty){
                    //Updates the icon and enables it.
                    this.m_Ícon.overrideSprite = ObservedItem.Icon;
                    this.m_Ícon.enabled = true;
                }else {
                    //If there is no item in this slot, disable icon
                    this.m_Ícon.enabled = false;
                }
            }

            if (this.m_Stack != null) {
                if (!IsEmpty){
                    //Updates the stack and enables it.
                    this.m_Stack.text = ObservedItem.Stack.ToString();
                    this.m_Stack.enabled = true;
                }else{
                    //If there is no item in this slot, disable stack field
                    this.m_Stack.enabled = false;
                }
            }
        }

        //Use the item
        public virtual void Use() {
            //Check if the item can be used.
            if (CanUse())
            {
                //Check if there is an override item behavior on trigger.
                if ((Trigger.currentUsedTrigger as Trigger) != null && (Trigger.currentUsedTrigger as Trigger).OverrideUse(this, ObservedItem))
                {
                    return;
                }
                if (Container.UseReferences)
                {
                    ObservedItem.Slot.Use();
                    return;
                }
                //Try to move item
                if (!MoveItem())
                {
                    ObservedItem.Use();
                }
            }
        }

        //Checks if we can use the item in this slot
        public virtual bool CanUse() {
            return true;
        }

        /// <summary>
        /// Try to move item by move conditions set in inspector
        /// </summary>
        /// <returns>True if item was moved.</returns>
        public virtual bool MoveItem() {
            if (Container.MoveUsedItem)
            {
                for (int i = 0; i < Container.moveItemConditions.Count; i++)
                {
                    ItemContainer.MoveItemCondition condition = Container.moveItemConditions[i];
                    ItemContainer itemContainer = WidgetUtility.Find<ItemContainer>(condition.window);
                    if (itemContainer == null || (condition.requiresVisibility && !itemContainer.IsVisible))
                    {
                        continue;
                    }
                    if (itemContainer.IsLocked) {
                        InventoryManager.Notifications.inUse.Show();
                        continue;
                    }
                    if (itemContainer.CanAddItem(ObservedItem) && itemContainer.StackOrAdd(ObservedItem))
                    {
                        if (!itemContainer.UseReferences || !Container.CanReferenceItems){
                            Container.RemoveItem(Index);
                        }
                        return true;
                    }
                    for (int j = 0; j < itemContainer.Slots.Count; j++)
                    {
                        if (itemContainer.CanSwapItems(itemContainer.Slots[j],this) && itemContainer.SwapItems(itemContainer.Slots[j], this))
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        /// <summary>
        /// Can the item be added to this slot. This does not check if the slot is empty.
        /// </summary>
        /// <param name="item">The item to test adding.</param>
        /// <returns>Returns true if the item can be added.</returns>
        public virtual bool CanAddItem(Item item)
        {
            for (int i = 0; i < restrictions.Count; i++)
            {
                if (!restrictions[i].CanAddItem(item))
                {
                  //  Debug.Log("Can't add item: "+item.Name+" to "+Container.Name+" Failed in: "+restrictions[i]);
                    return false;
                }
            }
            return true;
        }



    }
}